#!/bin/bash

for i in `seq 1 13`;
do
	echo Starting level $i

	luajit driver.lua $i 0 6 &
	luajit driver.lua $i 1 6 &
	luajit driver.lua $i 2 6 &
	luajit driver.lua $i 3 6 &
	luajit driver.lua $i 4 6 &
	luajit driver.lua $i 5 6 &

	wait
	echo All done for level $i
done
