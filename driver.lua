local yahtzee = require "yahtzee"
local compute_for_boxes = require("round_by_round").compute_for_boxes
local utils = require "utils"

local function run(num_boxes, start, skip)
	print("Running: ", num_boxes, start, skip)
	
	local box_sets = utils.subsets(yahtzee.boxes)

	-- sort box sets by count

	local counts = {}
	for i,v in ipairs(box_sets) do
		counts[#v] = counts[#v] or {}
		table.insert(counts[#v], v)
	end

	-- load relevant previous results

	local values = {}
	
	for i,v in ipairs(box_sets) do
		
		if #v == num_boxes then
			values[i] = utils.load("values_" .. i, true)
			print("Skipped " .. i)
		elseif	#v == num_boxes - 1 then
			values[i] = utils.load("values_" .. i)
			print("Loaded " .. i)
		end
		
	end
	
	-- compute

	for i,allowed_boxes in ipairs(counts[num_boxes]) do
		
		if i % skip == start then
			local allowed_boxes_set = utils.list_to_set(allowed_boxes)
			local boxes_index = utils.set_to_index(yahtzee.boxes, allowed_boxes_set)

			if not values[boxes_index] then
				compute_for_boxes(allowed_boxes, values)
			end
			
			-- save memory
			
			values[boxes_index] = nil
		end
	end
end

-- Read command line

--a,b,c = ...
--run(tonumber(a),tonumber(b),tonumber(c))

run(1,0,1)