local D = {}

-- Table operations

local function table_product(t)
	local res = 1
	for i,v in ipairs(t) do
		res = res * v
	end
	return res
end

local function table_copy(t)
	local res = {}
	for k,v in pairs(t) do
		res[k] = v
	end
	return res
end

local function treat_args(...)
	local args = {...}
	local func = args[#args]
	args[#args] = nil

	local is_table = false

	if #args == 1 and getmetatable(args[1]) ~= D then
		args = args[1]
		is_table = true
	end

	return args, func, is_table
end

-- Core functions

function D.new()
	local d = {}
	d.outcomes = {}
	d.numerical = true
	setmetatable(d, D)
	return d
end

function D.add_outcome(d, outcome, proba)
	
	-- adding a distribution
	if getmetatable(outcome) == D then
		
		for k,v in pairs(outcome.outcomes) do
			d:add_outcome(k, v * proba)
		end
		
	-- adding a normal outcome
	else
		d.outcomes[outcome] = (d.outcomes[outcome] or 0) + proba

		if type(outcome) ~= "number" then
			d.numerical = false
		end
	end
end

-- Useful distributions

function D.uniform(start, finish)

	if finish == nil then
		finish = start
		start = 1
	end
	local count = finish - start + 1
	local proba = 1 / count
	
	local d = D.new()
	for i = start,finish do
		d:add_outcome(i, proba)
	end
	return d
end

function D.arbitrary(outcomes)
	local d = D.new()
	local proba = 1 / #outcomes
	
	for i,v in ipairs(outcomes) do
		d:add_outcome(v, proba)
	end
	return d
end

function D.dup(dist, n)
	local res = {}
	for i = 1,n do
		res[i] = dist
	end
	return res
end

-- Operations on distributions

local function combine(distributions, fun)

	local d = D.new()
	local tempk = {}
	local tempv = {}

	local function rec(level)

		for k,v in pairs(distributions[level].outcomes) do
			tempk[level] = k
			tempv[level] = v

			if level == #distributions then
				local res = fun(tempk)
				d:add_outcome(res, table_product(tempv))
			else
				rec(level + 1)
			end
		end

	end

	rec(1)
	return d
end

function D.apply(...)
	local dists, func, was_table = treat_args(...)

	if was_table then
		return combine(dists, func)
	else
		return combine(dists, function(t) return func(table.unpack(t)) end)
	end
end

-- Shortcuts

function D.accumulate(distributions, fun)
	local d = distributions[1]
	for i = 2, #distributions do
		d = D.apply(d, distributions[i], fun)
	end
	return d
end

local wrap_op = function(op)
	return function(d1, d2)
		d1 = type(d1) == "number" and D.arbitrary{d1} or d1
		d2 = type(d2) == "number" and D.arbitrary{d2} or d2
		return D.apply(d1, d2, op)
	end
end

D.add = wrap_op(function(x,y) return x + y end)
D.div = wrap_op(function(x,y) return math.floor(x / y) end)
D.sub = wrap_op(function(x,y) return x - y end)
D.eq = wrap_op(function(x,y) return x == y end)
D.neq = wrap_op(function(x,y) return x ~= y end)
D.lt = wrap_op(function(x,y) return x < y end)
D.lte = wrap_op(function(x,y) return x <= y end)
D.gt = wrap_op(function(x,y) return x > y end)
D.gte = wrap_op(function(x,y) return x >= y end)

function D.mul(d1,d2)

	d2 = type(d2) == "number" and D.arbitrary{d2} or d2

	-- iterated sum as in 3d6 == d6 + d6 + d6
	if type(d1) == "number" then
		return D.sum(d2:dup(d1))

		-- combine for multiplication
	else
		return D.apply(d1,d2, function(x,y) return x*y end)
	end
end

function D.sum(distributions)
	return D.accumulate(distributions, function(x,y) return x+y end)
end

function D.count(...)
	local dists, predicate = treat_args(...)

	local counted = {}
	for i,dist in ipairs(dists) do
		counted[i] = dist:apply(function(x) return predicate(x) and 1 or 0 end)
	end
	return D.sum(counted)
end

function D.apply_sorted(...)
	local dists, func, was_table = treat_args(...)

	return D.apply(dists, function(t)
		local t = table_copy(t)
		table.sort(t)
		return was_table and func(t) or func(table.unpack(t))
	end)
end

-- Statistics

function D.average(d)

	if not d.numerical then return nil end

	local sum = 0

	for k,v in pairs(d.outcomes) do
		sum = sum + k * v
	end

	return sum
end

function D.deviation(d)

	if not d.numerical then return nil end

	local average = d:average()
	local variance = d:apply(function(x) return (x - average) ^ 2 end):average()

	return math.sqrt(variance)
end

function D.sorted_outcomes(d)

	local outcomes = {}
	for k,v in pairs(d.outcomes) do
		table.insert(outcomes, {k,v})
	end

	if d.numerical then
		table.sort(outcomes, function(t1, t2) return t1[1] < t2[1] end)
	end

	return outcomes
end

function D.cdf(d)

	if not d.numerical then
		return nil
	end

	local outcomes = d:sorted_outcomes()
	local cdf = {}

	cdf[1] = outcomes[1]

	for i = 2,#outcomes do
		local val = outcomes[i][2] + cdf[#cdf][2]
		table.insert(cdf, {outcomes[i][1], val})
	end

	return cdf
end

function D.find_cdf_value(cdf, value)
	
	for i,v in ipairs(cdf) do		
		if v[2] >= value then
			return v[1]
		end		
	end
end

function D.nth_iles(d, n)
	
	local cdf = d:cdf()
	local res = {}
	
	for i = 1,n-1 do
		res[i] = D.find_cdf_value(cdf, i/n)
	end

	return res
end

function D.median(d)
	return d:nth_iles(2)[1]
end

function D.median_absolute_deviation(d)
	
	local median = d:median()
	local deviation = d:apply(function(x) return math.abs(x - median) end)
	return deviation:median()
	
end

-- Pretty printing

function D.summary(d)
	
	local res = ""
	local outcomes = d:sorted_outcomes()

	for i,v in pairs(outcomes) do
		res = res .. tostring(v[1]) .. "\t" .. string.format("%5.2f%%", v[2] * 100) .. "\n"
	end

	if d.numerical then
		res = res .. "Average: " .. d:average() .. ", deviation: " .. d:deviation() .. "\n"
		res = res .. "Median: " .. d:median() .. ", MAD: " .. d:median_absolute_deviation() .. "\n"
		res = res .. "Quartiles: " .. table.concat(d:nth_iles(4), ", ") .. "\n"
	end
	
	return res
end

function D.print_cdf(d)

	if not d.numerical then return end

	local cdf = d:cdf()
	for i,v in ipairs(cdf) do
		print(v[1], string.format("%5.2f%%", v[2] * 100))
	end
end

function D.probability(d, outcome)
	return d.outcomes[outcome] or 0
end

-- Metatable

D.__index = D
D.__add = D.add
D.__mul = D.mul
D.__sub = D.sub
D.__div = D.div
D.__tostring = D.summary

return D