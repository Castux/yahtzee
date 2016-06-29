local y = require "yahtzee"
local serpent = require "serpent"
local mdp = require "mdp"
local utils = require "utils"

-- Compute all possible throws

local throws = y.roll("")

local dice_states = {}
for throw,prob in pairs(throws) do
	table.insert(dice_states, throw)
end

-- All possible combinations of boxes

local box_states = utils.subsets(y.boxes)
local tmp = {}
for i,v in pairs(box_states) do
	box_states[i] = utils.list_to_set(v)
end

-- Remove the game over state

box_states[0] = nil

-- And combine all together with reroll stages

local states = {}
local states_list = {}

for box_state_index,_ in ipairs(box_states) do
	states[box_state_index] = {}
	
	for roll = 1,4 do
		states[box_state_index][roll] = {}
		
		for _,dice_state in ipairs(dice_states) do
			local state = 
			{
				boxes = box_state_index,
				roll = roll,
				throw = dice_state				
			}
			
			states[box_state_index][roll][dice_state] = state
			table.insert(states_list, state)
		end
	end
end

table.insert(states_list, "game over")

-- Make Markov decision process

local function compute_actions(state)
	
	local actions = {}
		
	-- in all cases you can score
	
	for box,v in pairs(box_states[state.boxes]) do
		assert(v == true)
		
		-- we're scoring box, which gives us a reward
		local reward = y.score(state.throw, box)

		-- the box also becomes checked
		local next_boxes = utils.table_copy(box_states[state.boxes])
		next_boxes[box] = nil
		next_boxes = utils.set_to_index(y.boxes, next_boxes)

		local action = {}
		
		-- not game over? reroll all dice
		
		if next_boxes > 0 then
			
			for throw,proba in pairs(y.roll("")) do
				local next_state = states[next_boxes][1][k]
				table.insert(action, { next_state = next_state, probability = proba, reward = reward })
			end
		
		else
			table.insert(action, { next_state = "game over", probability = 1, reward = reward })
		end
		
		actions[box] = action
	end
	
	-- if it's not the fourth roll, you can also reroll
	
	if state.roll < 4 then
		for reroll,outcomes in pairs(y.rerolls(state.throw)) do
			local action = {}
			
			for next_dice,proba in pairs(outcomes) do
				
				local next_state = states[state.boxes][state.roll + 1][next_dice]
				table.insert(action, { next_state = next_state, probability = proba, reward = 0 })
			end
			
			actions[reroll] = actions
		end
	end
	
	return actions
end

for i,state in pairs(states_list) do
	
	if state ~= "game over" then
		state.actions = compute_actions(state)
	end
	
end

print(#states_list)