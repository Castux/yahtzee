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
-- next_result as a function(upper_score, dice, allowed_boxes)

function compute(phase, upper_score, dice, allowed_boxes, next_results)

	local max_value = -1
	local max_action

	-- Scoring only

	if phase == 4 then


		for box,_ in pairs(allowed_boxes) do

			-- choosing this box gives us what reward?

			local score = yahtzee.score(dice, box)

			-- upper bonus?

			local new_upper_score = math.min(63, upper_score + score)

			if upper_score < 63 and new_upper_score == 63 then
				score = score + 35
			end

			-- add the future expected value

			local new_allowed_boxes = utils.table_copy(allowed_boxes)
			new_allowed_boxes[box] = nil

			local future_score = 0

			for reroll,proba in pairs(yahtzee.roll("")) do
				local value = next_results(new_upper_score, reroll, new_allowed_boxes)
				future_score = future_score + proba * value
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



	end

	return max_action, max_value

end

local function run()

	local box_sets = utils.subsets(yahtzee.boxes)
	box_sets[0] = nil	-- remove the "no boxes" set

	-- sort box sets by count

	table.sort(box_sets, function(a,b)
		return #a < #b
	end)

	local results = {}

	for _,allowed_boxes in ipairs(box_sets) do
		
		local allowed_boxes_set = utils.list_to_set(allowed_boxes)
		local boxes_hash = table.concat(allowed_boxes, ",")
		
		for phase = 4,1,-1 do
			for upper_score = 0,63 do
				for dice,_ in pairs(yahtzee.roll("")) do
					local action, value = compute(phase, upper_score, dice, allowed_boxes_set, function() return 0 end)
					--print(boxes_hash, phase, upper_score, dice, action, value)
				end
			end
		end
	end
end

run()