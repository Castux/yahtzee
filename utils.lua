--[[ Utilities ]]--

local function memoize(func)

	local cache = {}
	
	return function(arg)
		if cache[arg] ~= nil then
			return cache[arg]
		end
		
		local result = func(arg)
		
		cache[arg] = result
		return result		
	end
end

-- returns the n-th subset of list t
local function subset(t, n)
	
	local res = {}
	for i = 1, #t do
		
		if n % 2 == 1 then
			table.insert(res, t[i])
		end
		
		n = math.floor(n / 2)		
	end
	
	return res
end

local function subsets(t)
	
	local result = {}
	
	for i = 0, 2 ^ #t - 1 do
		result[i] = subset(t,i)
	end
	
	return result
end

local function list_to_set(t)
	local set = {}
	for _,v in ipairs(t) do
		set[v] = true
	end
	return set
end

local function set_to_index(list, set)
	local index = 0
	
	for i,v in ipairs(list) do
		if set[v] then
			index = index + 2 ^ (i-1)
		end
	end
	
	return index
end

local function string_to_numbers(s)

	local t = {}
	for i = 1,#s do
		t[i] = tonumber(s:sub(i,i))
	end
	return t
end

local function sort_string(s)
	local t = string_to_numbers(s)
	table.sort(t)
	return table.concat(t)
end

local function table_copy(t)
	local copy = {}
	for k,v in pairs(t) do
		copy[k] = v
	end
	return copy
end

return
{
	memoize = memoize,
	subset = subset,
	subsets = subsets,
	list_to_set = list_to_set,
	string_to_numbers = string_to_numbers,
	sort_string = sort_string,
	set_to_index = set_to_index,
	table_copy = table_copy
}