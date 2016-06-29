local mdp = {}

function mdp.new()
	
	local self = {}
	self.states = nil		-- array of states
	self.actions = nil		-- function(state)
	self.transition = nil	-- probability, reward = function(state,action,state)
	self.discount = 1
	
	setmetatable(self, mdp)
	return self	
end


-- Module

mdp.__index = mdp

return mdp