set FileDate=%DATE:~10,4%%DATE:~4,2%%DATE:~7,2%

hg log --template "{date|isodatesec}, {author|user}, {desc|strip}\n" > %FileDate%_log.csv

pause