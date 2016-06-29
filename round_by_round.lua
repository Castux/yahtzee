local yahtzee = require "yahtzee"
local utils = require "utils"

-- Round by round computation

-- Each round is actually split in 4 phases (the rerolls)
-- Between each phase you can reroll (keeping 0-5 dice)
-- After phase 4 you need to score

-- During a round N, you have 14 - N boxes available among 13,
-- or C(13,14-N) = C(13, N-1)

-- In each phase there are 252 possible dice states, and 1+63 possible current upstairs scores,
-- that is 16128 states.

-- There are 2^13 box states (-1 for the game over when all is full), and 4 rounds per phase so a grand total of
-- 528417792 states to compute. But since every action makes you move forward in phase and/or round,
-- they can be nicely split by phase and computed backwards, saving only the optimal strategies.

-- Check all possible actions, and compute their expected value thanks to the
-- precomputed next_results

-- allowed_boxes as a set {[box] = true}
-- next_result as a function(allowed_boxes, round, upper_score, dice)

function compute(phase, upper_score, dice, allowed_boxes, next_results)

	local max_value = -1
	local max_action

	-- Scoring only

	if phase == 4 then


		for box,_ in pairs(allowed_boxes) do

			-- choosing this box gives us what reward?

			local score = yahtzee.score(dice, box)

			-- upper bonus?

			if yahtzee.is_upper_box(box) then
				local new_upper_score = math.min(63, upper_score + score)

				if upper_score < 63 and new_upper_score == 63 then
					score = score + 35
				end
			end

			-- add the future expected value

			local new_allowed_boxes = utils.table_copy(allowed_boxes)
			new_allowed_boxes[box] = nil

			local future_score = 0

			if not utils.empty_set(new_allowed_boxes) then

				for reroll,proba in pairs(yahtzee.roll("")) do
					local value = next_results(new_allowed_boxes, 1, new_upper_score, reroll)
					future_score = future_score + proba * value
				end

			end

			-- total

			local total_value = score + future_score
			if total_value > max_value then
				max_value = total_value
				max_action = box
			end
		end


		-- Rerolling only

	else

		for keep,outcomes in pairs(yahtzee.rerolls(dice)) do

			local future_value = 0

			for rerolled,proba in pairs(outcomes) do
				local value = next_results(allowed_boxes, phase + 1, upper_score, rerolled)
				future_value = future_value + proba * value
			end

			if future_value > max_value then
				max_value = future_value
				max_action = "keep " .. keep
			end
		end

	end

	return max_action, max_value

end

local function compute_for_boxes(allowed_boxes, results)

	local next_results = function(allowed_boxes, phase, upper_score, dice)
		local index = utils.set_to_index(yahtzee.boxes, allowed_boxes)
		return results[index][phase][upper_score][dice].value
	end

	local allowed_boxes_set = utils.list_to_set(allowed_boxes)
	local boxes_index = utils.set_to_index(yahtzee.boxes, allowed_boxes_set)

	-- new results

	results[boxes_index] = {
		boxes_hash = table.concat(allowed_boxes, ","),
		boxes_index = boxes_index
	}

	for phase = 4,1,-1 do

		results[boxes_index][phase] = {}

		for upper_score = 0,63 do

			results[boxes_index][phase][upper_score] = {}

			for dice,_ in pairs(yahtzee.roll("")) do

				local action, value = compute(phase, upper_score, dice, allowed_boxes_set, next_results)
				results[boxes_index][phase][upper_score][dice] = { action = action, value = value }
			end
		end
	end

	utils.dump(results[boxes_index], "boxes_" .. boxes_index)
	print("Done with " .. results[boxes_index].boxes_hash)		

end


local function run()

	local box_sets = utils.subsets(yahtzee.boxes)
	box_sets[0] = nil	-- remove the "no boxes" set

	-- sort box sets by count

	table.sort(box_sets, function(a,b)
		return #a < #b
	end)

	local results = {}

	-- load precomputed results

	for i = 1,#box_sets do
		local res = utils.load("boxes_" .. i)
		if res then
			results[i] = res
		end
	end

	-- compute

	for _,allowed_boxes in ipairs(box_sets) do

		--[[ memory management: we need only the results for next round

			for i,v in ipairs(box_sets) do
				if #v < #allowed_boxes - 1 then
					results[table.concat(v, ",")] = nil
				end
			end]]

		local allowed_boxes_set = utils.list_to_set(allowed_boxes)
		local boxes_index = utils.set_to_index(yahtzee.boxes, allowed_boxes_set)
		
		if not results[boxes_index] then
			compute_for_boxes(allowed_boxes, results)
		end
	end
end

run()