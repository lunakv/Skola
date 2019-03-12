% 1. domácí úloha
% Václav Luňák
%
% a) Implementujte logaritmus o základu 2 (dolní celou část) na unárně 
% reprezentovaných číslech.
%
% logtwo(+N, ?Vysledek)
logtwo(s(0), 0).
logtwo(N, s(R)) :- N \= 0, half(N, X), logtwo(X, R).

% b) Implementujte predikát, který spočte n-té Fibonacciho číslo lépe než
% v exponenciálním čase (ideálně pouze lineárně mnoho sčítání).
%
% fib(+N, ?Vysledek)

% Zobecněný Fibonacci
%
% generalFib(+N, +F_0, +F_1, ?Vysledek)
generalFib(N, F, S, R) :- linearGenFib(N, F, S, R, _).

% Lineární zobecněný Fibonacci
% Vrací poslední i předposlední hodnotu -> na každé úrovni je jen jedno
% rekurzivní volání -> stačí pouze lineární počet sčítání.
%
% linearGenFib(+N, +F_0, +F_1, ?F_n, ?F_(n-1))
linearGenFib(0, F, _, F, 0).
linearGenFib(1, F, S, S, F).

% R = Result   = F_n, 
% L = Last     = F_n-1, 
% P = Previous = F_n-2
linearGenFib(N, F, S, R, L) :- 
        N > 1, N1 is N - 1,
        linearGenFib(N1, F, S, L, P),
        R is L + P.


%-----------------------------------------------------------------------
% Pomocné predikáty.
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

% Půlení.
half(0, 0).
half(s(0), 0).
half(s(s(X)), s(R)) :- half(X, R).

