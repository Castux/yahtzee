local yahtzee = require "yahtzee"
local utils = require "utils"

local num_boxes = 2 ^ #yahtzee.boxes
local num_phases = 4
local num_upper_scores = 64

local rolls = {}
for k,_ in pairs(yahtzee.roll("")) do
	table.insert(rolls, k)
end
table.sort(rolls)

local reverse_rolls = {}
for i,v in ipairs(rolls) do
	reverse_rolls[v] = i
end

local num_rolls = #rolls

local function index(phase, upper_score, dice)
	return phase * num_upper_scores * num_rolls +
		upper_score * num_rolls +
		reverse_rolls[dice]
end


local function get(array, phase, upper_score, dice)
	return array[index(phase, upper_score, dice)]
end

local function set(array, phase, upper_score, dice, value)
	array[index(phase, upper_score, dice)] = value
end

return
{
	get = get,
	set = set
}
