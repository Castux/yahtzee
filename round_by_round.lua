local yahtzee = require "yahtzee"
local utils = require "utils"
local storage = require "storage"

-- Round by round computation

-- Each round is actually split in 3 phases (the rerolls)
-- Between each phase you can reroll (keeping 0-5 dice)
-- After phase 3 you need to score

-- During a round N, you have 14 - N boxes available among 13,
-- or C(13,14-N) = C(13, N-1)

-- In each phase there are 252 possible dice states, and 1+63 possible current upstairs scores,
-- that is 16128 states.

-- There are 2^13 box states (-1 for the game over when all is full), and 3 phases per round so a grand total of
-- 396361728 states to compute. But since every action makes you move forward in phase and/or round,
-- they can be nicely split by phase and computed backwards, saving only the optimal strategies.

-- Check all possible actions, and compute their expected value thanks to the
-- precomputed next_results

-- allowed_boxes as a set {[box] = true}
-- next_result as a function(allowed_boxes, round, upper_score, dice)

local function compute(phase, upper_score, dice, allowed_boxes, next_results)

	local max_value = -1
	local max_action

	-- Scoring only

	if phase == 2 then


		for box,_ in pairs(allowed_boxes) do

			-- choosing this box gives us what reward?

			local score = yahtzee.score(dice, box)

			-- upper bonus?
			local new_upper_score = upper_score
			if yahtzee.is_upper_box(box) then
				new_upper_score = math.min(63, upper_score + score)

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
				max_action = keep
			end
		end

	end

	return max_action, max_value

end

local function compute_for_boxes(allowed_boxes, values)

	local next_results = function(allowed_boxes, phase, upper_score, dice)
		local index = utils.set_to_index(yahtzee.boxes, allowed_boxes)
		return storage.get(values[index], phase, upper_score, dice)
	end

	local allowed_boxes_set = utils.list_to_set(allowed_boxes)
	local boxes_index = utils.set_to_index(yahtzee.boxes, allowed_boxes_set)

	-- new results
	
	local actions = {}
	local these_values = {}
	values[boxes_index] = these_values
	
	print("Starting " .. boxes_index .. "...")

	for phase = 2,0,-1 do

		for upper_score = 0,63 do

			for dice,_ in pairs(yahtzee.roll("")) do

				local action, value = compute(phase, upper_score, dice, allowed_boxes_set, next_results)
				
				storage.set(actions, phase, upper_score, dice, action)
				storage.set(these_values, phase, upper_score, dice, value)
			end
		end
	end

	utils.dump(actions, "actions_" .. boxes_index)
	utils.dump(these_values, "values_" .. boxes_index)
	
	print("Done with " .. boxes_index)
end

return { compute_for_boxes = compute_for_boxes }