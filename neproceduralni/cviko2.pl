nat(0).
nat(s(X)) :- nat(X).

% Méně než.
lt(0, s(Y)) :- nat(Y).
lt(s(X), s(Y)) :- lt(X, Y).

add(0, Y, Y) :- nat(Y).
add(s(X), Y, s(Z)) :- add(X, Y, Z).

subtract(X, Y, R) :- add(Y, R, X).

% div(+X, +Y, ?D, ?R)
div(X,Y,0,X) :- lt(X,Y).
div(X,Y,s(D),R) :- subtract(X,Y,R2), div(R2, Y, D, R).

% pomocné příkazy
toNat(N, R) :-
    integer(N),
    toNat_(N, R).
  
toNat_(N, R) :- N > 0 ->
(N2 is N - 1, toNat_(N2, R2), R = s(R2));
R = 0.

fromNat(0, 0).
fromNat(s(N), R) :-
fromNat(N, R2),
R is R2 + 1.
