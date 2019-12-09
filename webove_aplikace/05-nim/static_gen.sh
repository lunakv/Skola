#!/bin/sh

for i in $(seq 0 20); do
for j in 1 2; do

if [ "$j" = 1 ]; then
	n=2
else
	n=1
fi

echo "
<!doctype html>
<html>
<head>
	<meta charset=\"utf-8\">
	<title>NIM</title>
	<link rel=\"stylesheet\" href=\"style.css\" type=\"text/css\">
</head>

<body>
<h1>NIM</h1>
<p>Players take 1-3 matches in turns. Whoever takes the last match, looses.</p>

<p><strong>Player $j</strong> is on the move.</p>
<div class=\"center\">" >> "$i"_"$j".html

# Victory condition
if [ "$i" = 0 ]; then
	echo "
<p>There are no matches left...</p
<p>Game over. The winner is <strong>Player "$j"</strong>!<br><a href=\"20_1.html\">Play Again</a></p>" >> "$i"_"$j".html
else

if [ $i -lt 3 ]; then 
	l=$i
else
	l=3
fi
for k in $(seq $l); do
	echo "<a href=\"$(($i - $k))_$n.html\" class=\"match\">" >> "$i"_"$j".html
	echo "<img src=\"pic/match.png\" class=\"match\">" >> "$i"_"$j".html
	echo "</a>" >> "$i"_"$j".html
done
for k in $(seq $(($i - $l))); do
	echo "<img src=\"pic/match.png\" class=\"match\">" >> "$i"_"$j".html
done

fi

echo "
</div>
</body>
</html>" >> "$i"_"$j".html

done
done


