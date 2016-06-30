local yahtzee = require "yahtzee"
local compute_for_boxes = require("round_by_round").compute_for_boxes
local utils = require "utils"

local function run(num_boxes, start, skip)
	
	print("Running: ", num_boxes, start, skip)
	
	local box_sets = utils.subsets(yahtzee.boxes)

	-- sort box sets by count and load/check existence

	local counts = {}
	local values = {}
	
	for i = 1, yahtzee.box_set_builder.max_index do
		
		local list = yahtzee.box_set_list(i)
		
		counts[#list] = counts[#list] or {}
		table.insert(counts[#list], i)
		
		if #list == num_boxes then
			values[i] = utils.load("values_" .. i, true)
			if values[i] then
				print("Skipped " .. i)
			end
		elseif #list == num_boxes - 1 then
			values[i] = utils.load("values_" .. i)
			if values[i] then
				print("Loaded " .. i)
			end
		end
	end
	
	-- compute

	for i,box_set in ipairs(counts[num_boxes]) do
		
		if i % skip == start then
			
			if not values[box_set] then
				compute_for_boxes(box_set, values)
			end
			
			-- save memory
			
			values[box_set] = nil
		end
	end
end

-- Read command line

a,b,c = ...

if not a or not b or not c then
	for i = 1,13 do
		run(i,0,1)
	end
else
	run(tonumber(a),tonumber(b),tonumber(c))
end
