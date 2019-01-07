head -n 8 < A.in | tail -n +4 > A.out

head -n 1 < B.in | rev > B.out
tail -n +2 < B.in >> B.out

tail -n 5 < C.in | tac > C.out

tac < D.in | tr "a-zA-Z" "n-za-mN-ZA-M" > D.out