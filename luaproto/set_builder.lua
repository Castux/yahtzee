local set_builder = {}

-- sets are just numbers (their "index")
-- Can't have more than like 52 elements in a set

function set_builder.new(list)
	
	local sb = {}
	sb.list = list
	
	sb.values = {}
	for i,v in ipairs(list) do
		sb.values[v] = 2 ^ (i-1)
	end
	
	sb.max_index = 2 ^ #list - 1
	
	setmetatable(sb, set_builder)
	return sb
end

function set_builder:empty()
	return 0
end

function set_builder:add(set, elem)
	
	assert(not self:contains(set, elem))
	
	return set + self.values[elem]
end

function set_builder:remove(set, elem)
	
	assert(self:contains(set, elem))
	
	return set - self.values[elem]
end

function set_builder:contains(set, elem)
	
	return math.floor( set / self.values[elem] ) % 2 == 1
end

function set_builder:elements(set)
	
	local t = {}
	
	for i,v in ipairs(self.list) do
		if self:contains(set, v) then
			table.insert(t, v)
		end
	end
	
	return t
end

set_builder.__index = set_builder

return set_builder