local distribution = require "distribution"
local utils = require "utils"
local set_builder = require "set_builder"

--[[ Random rolling ]]--

local roll = utils.memoize(function(keep)
	
	keep = keep or ""
	
	if #keep == 5 then
		return { [keep] = 1 }
	end
	
	local d6 = distribution.uniform(6)
	
	return distribution.apply(d6:dup(5 - #keep), function(t)
		return utils.sort_string(table.concat(t) .. keep)
	end).outcomes
end)

local rerolls = utils.memoize(function(throw)
	
	assert(#throw == 5, "Can only reroll a full set of dice")
	
	local rerolls = {}
	local faces = utils.string_to_numbers(throw)
	
	for i,v in pairs(utils.subsets(faces)) do
		local keep = table.concat(v)
		rerolls[keep] = roll(keep)
	end
	
	return rerolls
end)

--[[ Scoring ]]--

local boxes =
{
	"ones",
	"twos",
	"threes",
	"fours",
	"fives",
	"sixes",
	"three of a kind",
	"four of a kind",
	"full house",
	"small straight",
	"large straight",
	"yahtzee",
	"sum"
}

local box_set_builder = set_builder.new(boxes)

local function counts(throw)

	local c = {0,0,0,0,0,0}
	local sum = 0
	for i,v in ipairs(utils.string_to_numbers(throw)) do
		
		c[v] = c[v] + 1
		sum = sum + v
	end
	
	return c, sum
end

local function n_of_kind(counts, n, exact)

	exact = exact or false

	for i,v in ipairs(counts) do
		if (exact and v == n) or (not exact and v >= n) then
			return true
		end
	end
		
	return false
end

local function sequence(counts, length)
		
	for start = 1,7 - length do
		local all = true
		for i = 0,length - 1 do
			if counts[start + i] == 0 then
				all = false
				break
			end
		end
		
		if all then return true end
	end
	
	return false
end

local function score(throw, box)

	local c, sum = counts(throw)

	if box == "ones" then
		return c[1]
	
	elseif box == "twos" then
		return c[2] * 2
	
	elseif box == "threes" then
		return c[3] * 3
	
	elseif box == "fours" then
		return c[4] * 4
	
	elseif box == "fives" then
		return c[5] * 5
	
	elseif box == "sixes" then
		return c[6] * 6
	
	elseif box == "three of a kind" then
		return n_of_kind(c, 3) and sum or 0
	
	elseif box == "four of a kind" then
		return n_of_kind(c, 4) and sum or 0
		
	elseif box == "full house" then
		return (n_of_kind(c, 2, true) and n_of_kind(c, 3, true)) and 25 or 0
	
	elseif box == "small straight" then
		return sequence(c, 4) and 30 or 0
	
	elseif box == "large straight" then
		return sequence(c, 5) and 30 or 0
	
	elseif box == "yahtzee" then
		return n_of_kind(c, 5) and 50 or 0
		
	elseif box == "sum" then
		return sum
	
	end
	
	return 0
end

local function is_upper_box(box)
	return box == "ones" or
		box == "twos" or
		box == "threes" or
		box == "fours" or
		box == "fives" or
		box == "sixes"
end

--[[ Module exports ]]--

return
{
	boxes = boxes,
	box_set_builder = box_set_builder,
	box_set_list = utils.memoize(function(set) return box_set_builder:elements(set) end),
	roll = roll,
	rerolls = rerolls,
	score = score,
	is_upper_box = is_upper_box
}