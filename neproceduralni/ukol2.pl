% Václav Luňák
% 1)
% Creates a flatmap of a list
% Given an arbitrary list X, R is a one-dimensional list containing all elemens of X
% in the original order, ignoring empty lists. 
%
% flat(+X, ?R)
flat([], []).
flat(X, R) :- flat_(X, [], R).


flat_([], A, R) :- rev(A, R).         % if input is empty, just reverse accumulator into solution
flat_([H|T], A, R) :-                   
    isEmpty(H) -> flat_(T, A, R);     % if first item is an empty list (see below), discard it
    H \= [_|_] -> flat_(T, [H|A], R); % if first item is not a list, put it to accumulator 
        flat(H, HF), revCopy(HF, A, A2), flat_(T, A2, R).
                                      % otherwise flatten the head and copy it to accumulator


% Decides whether the argument is an empty list. 
% A list is considered empty if its flatmap is equal to []
%
% isEmpty(+X).
isEmpty([]).
isEmpty([X|Y]) :- isEmpty(X), isEmpty(Y).

% Copies a list to the beginning of another list in reverse order.
% Useful for copying lists at the "end" of accumulators
%
% revCopy(+X, +A, -R)
revCopy([], A, A).
revCopy([H|HS], A, R) :- revCopy(HS, [H|A], R).


% 2)
% Transposes a matrix
% Matrix must be given as a list of lists, with each inner list 
% having the same length. Otherwise the conversion will fail.
%
% transp(+X, ?R)
transp(X, []) :- isEmpty(X), !.
transp(M, R) :-
    getHeads(M, H, Re), transp(Re, T), R = [H|T].

% Given a list of lists, separates the first element of each 
% inner list from the rest.
%
% getHeads(+L, -Heads, -Rest)
getHeads([], [], []).
getHeads([H|T], [R|R1], [Z|Z1]) :- H = [R|Z], getHeads(T, R1, Z1).


%                    Pomocné funkce z cvičení                   
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
rev(XS, R) :- rev_(XS, [], R).

rev_([], A, A).
rev_([X|XS], A, R) :-
  rev_(XS, [X|A], R).