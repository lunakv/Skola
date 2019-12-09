#!/bin/bash

printf "Content-type: text/html\r\n\r\n"

# Get number of matches from URL query string
matches=20
player=0
n=1
taken=0
for token in ${QUERY_STRING//&/ }; do
	key_value=( ${token/=/ } );
	if [[ ${key_value[0]} == 'matches' ]]; then
		matches=${key_value[1]}
	elif [[ ${key_value[0]} == 'player' ]]; then
		player=${key_value[1]}
		n=$((1 - $player))
	elif [[ ${key_value[0]} == 'taken' ]]; then
		taken=${key_value[1]}
	fi
done

if [ "$matches" -lt 3 ]; then
	m="$matches"
else
	m=3
fi

echo "
<!doctype html>
<html>
<head>
	<meta charset=\"utf-8\">
	<title>NIM</title>
	<!-- TODO: Link may need fixing (depending on your location of styles and images) -->
	<link rel=\"stylesheet\" href=\"../nim/style.css\" type=\"text/css\">
</head>

<body>
<h1>NIM</h1>
<p>Players take 1-3 matches in turns. Whoever takes the last match, looses.</p>
<p><strong>Player $(($player + 1))</strong> is on the move.</p>"

# Game over
if [ "$matches" -eq 0 ]; then
	echo '<p>Game over. The winner is <strong>Player '"$(($player + 1))"'</strong><br><a href="?player=0&matches=20">Play Again...</a></p>'
	
else
	echo '<div class="center">'
	# Last taken matches
	for i in $(seq "$taken"); do
		echo '<img src="../nim/pic/match.png" class="match taken">'
	done
	# Clickable matches
	for i in $(seq "$m"); do
		echo "<a href=\"?taken=$i&player=$n&matches=$(( "$matches" - $i ))\" class=\"match\">"
		echo '<img src="../nim/pic/match.png" class="match">'
		echo '</a>'
	done
	# Remaining matches
	for i in $(seq $(( "$matches" - "$m" ))); do
		echo '<img src="../nim/pic/match.png" class="match">'
	done
	echo '</div>'
fi

echo '</body>'
echo '</html>'
