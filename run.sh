#!/bin/bash
LEVEL=3
luajit driver.lua $LEVEL 0 4 & luajit driver.lua $LEVEL 1 4 & luajit driver.lua $LEVEL 2 4 & luajit driver.lua $LEVEL 3 4