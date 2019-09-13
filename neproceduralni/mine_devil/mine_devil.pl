/* ++ Core game loop ++ */ 
game :-
    welcomeScreen,
    getInput([D], 'Difficulty (1-3):'),
    D > 0, D < 4,!,
    trace,notrace,
    fillMines(D,G),
    writeln(G),
    revealAll(G,I),showGrid(I),
    size(G,L,W),
    firstReveal(L,W,G).

firstReveal(L, W, G) :-
    showGrid(G),
    getCoords(X,Y),
    validCoords(X,Y,L,W),!,
    safeReveal(X,Y,G,G2),
    fillNumbers(G2,G3),
    updateGrid(G3,H),
    loop(L,W,H).

loop(_, _, G) :- \+ unsolved(G), win(G),!.
loop(L, W, G) :-
    showGrid(G), 
    getCoords(X,Y),
    validCoords(X,Y,L,W),!,
    (
        dangerAt(X,Y,G), setLosingMine(X,Y,G,Loss), lose(Loss),!;
        reveal(X,Y,G,G2),
        updateGrid(G2,H),
        revealAll(H,I), showGrid(I),!,
        loop(L, W, H)
    ).

win(G) :-
    showGrid(G),
    writeln('CONGRATULATIONS! YOU WIN!').

lose(G) :-
    showGrid(G),
    writeln('GAME OVER. YOU LOST.').

tryEdge([],[]).
tryEdge([[]|T], [[]|RT]) :- tryEdge(T,RT).
tryEdge([[[V,S]|HT]|T], [[[V,S]|RHT]|RT]) :-
    (V = shown; V = deep; S = m; S = n),
    tryEdge([HT|T], [RHT|RT]).
tryEdge([[[edge,S]|HT]|T], [[[edge,N]|HT]|T]) :-
    S \= m, S \= n,
    (N = n; N = m).

dangerAt(X,Y,G) :-
    field(X,Y,G,[V,S]),
    (
        V = shown,!,false;
        V = deep;
        S = x
    ).
dangerAt(X,Y,G) :-
    markAsSureMine(X,Y,G,H), fillEdges(H,_).
/* -- Core game loop -- */


/* ++ Possibility search ++ */

% markSureMines(+G,-H)
% Sets 'm' mines around fields which have just enough covered neighbours
markSureMines([H|T],Res) :- 
    length(H,WW), W is WW-1, length(T,L),
    markSureMines_(L,W,[H|T],Res).
markSureMines_(X,_,G,G) :- X < 0.
markSureMines_(X,Y,G,H) :-
    X >= 0,
    markSureMinesR(X,Y,G,G2),
    X2 is X-1,
    markSureMines_(X2,Y,G2,H).
markSureMinesR(_,Y,G,G) :- Y < 0.
markSureMinesR(X,Y,G,H) :-
    field(X,Y,G,F),
    (
        F = [shown,N], N > 0, coveredAround(X,Y,G,N), markAround(X,Y,G,G2,m),!;
        G2 = G
    ),
    Y2 is Y-1,
    markSureMinesR(X,Y2,G2,H).

% markSureSafe(+G,-H)
% Sets 'n' nonmines around fields that have just enough 'm' neighbours
markSureSafe([H|T], Res) :-
    length(H,WW), W is WW-1, length(T,L),
    markSureSafe_(L,W,[H|T],Res).
markSureSafe_(X,_,G,G) :- X < 0.
markSureSafe_(X,W,G,H) :-
    X >= 0,
    markSureSafeR(X,W,G,G2),
    X2 is X-1,
    markSureSafe_(X2,W,G2,H).

markSureSafeR(_,Y,G,G) :- Y < 0.
markSureSafeR(X,Y,G,H) :-
    field(X,Y,G,F),
    (
        F = [shown,N], N > 0,sureMinesAround(X,Y,G,N), markAround(X,Y,G,G2,n),!;
        G2 = G
    ),
    Y2 is Y-1,
    markSureSafeR(X,Y2,G2,H).

% infer(+G,-H)
% Applies sure 'm' mines and sure 'n' nonmines as long as possible
infer(G,H) :-
    markSureMines(G,G2),
    markSureSafe(G2,G3),
    (
        G3 = G, H = G;
        G3 \= G, infer(G3,H)
    ).

% notOverflown(+X,+Y,+G,+F)
% Succeeds iff X,Y isn't too uncovered and doesn't have
% too many 'm' neighbours
% F MUST be equal to G[X,Y]
notOverflown(_,_,_,[V,_]) :- V \= shown.
notOverflown(X,Y,G,[shown,N]) :-
    sureMinesAround(X,Y,G,M),
    N >= M,
    coveredAround(X,Y,G,Z),
    Z >= N.

% withoutOverflows(+G)
% Succeeds iff G contains no overflows (see notOverflown/4)
withoutOverflows([H|T]) :-
    length(H,W),length([H|T],L),
    withoutOverflows_(0,0,[H|T],L,W).
withoutOverflows_(X,_,_,L,_) :-
    X >= L.
withoutOverflows_(X,Y,G,L,W) :-
    X < L, Y >= W, X2 is X+1,
    withoutOverflows_(X2,0,G,L,W).
withoutOverflows_(X,Y,G,L,W) :-
    X < L, Y < W, Y2 is Y+1,
    field(X,Y,G,F),
    notOverflown(X,Y,G,F),
    withoutOverflows_(X,Y2,G,L,W).

% setLosingMine(+X,+Y,+G,-H)
% Sets X,Y to [shown,x] -- used to show potential losing mine at end of game
setLosingMine(_,_,[],[]).
setLosingMine(0,Y,[H|T],[R|T]) :-
    setLosingMineR(Y,H,R).
setLosingMine(X,Y,[H|T],[H|RT]) :-
    X > 0, X2 is X-1,
    setLosingMine(X2,Y,T,RT).

setLosingMineR(_, [], []).
setLosingMineR(0, [_|T], [[shown,x]|T]).
setLosingMineR(Y, [H|T], [H|R]) :-
    Y > 0, Y2 is Y-1,
    setLosingMineR(Y2,T,R).


markAsSureMine(_,_,[],[]).
markAsSureMine(0,0,[[[V,_]|TH]|T], [[[V,m]|TH]|T]).
markAsSureMine(0,Y,[[H|HT]|T], [[H|RHT]|T]) :-
    Y > 0, Y2 is Y-1,
    markAsSureMine(0,Y2,[HT|T],[RHT|T]).
markAsSureMine(X,Y,[H|T], [H|TR]) :-
    X > 0, X2 is X-1,
    markAsSureMine(X2,Y,T,TR).


% markAround(+X,+Y,+G,-H,+M)
% Sets status of neighbours of X,Y to M,
% unless the neighbour is 'shown', 'n', or 'm'.
% Used for marking sure 'm' mines and 'n' nonmines on edges
markAround(_,_,[],[],_).
markAround(-1,Y,[H|T],[R|T],M) :- markAroundR(Y,H,R,M).
markAround(X,Y,[H|T],[R|RT],M) :-
    (
        X = 0, markAroundREx(Y,H,R,M);
        X = 1, markAroundR(Y,H,R,M);
        \+ diffOne(X,0), H = R
    ),
    X2 is X-1,
    markAround(X2,Y,T,RT,M).

markAroundR(-1, [[V,S]|T], [[V,N]|T], M) :- (V = shown,!; S = m; S = n), N = S; N = M.
markAroundR(Y, [[shown,S]|T], [[shown,S]|RT], M) :-
    Y > -1, Y2 is Y-1,
    markAroundR(Y2,T,RT,M).
markAroundR(Y, [[V,S]|T], [[V,S]|RT], M) :-
    V \= shown, (S = m; S = n),
    Y > -1, Y2 is Y-1,
    markAroundR(Y2,T,RT,M).
markAroundR(Y, [[V,S]|T], [[V,N]|RT], M) :-
    Y > -1, V \= shown, S \= m, S \= n,
    (
        (Y = 0;Y = 1), N = M;
        \+ diffOne(Y,0), N = S
    ),
    Y2 is Y-1,
    markAroundR(Y2,T,RT,M).

markAroundREx(-1, [[V,S]|T], [[V,N]|T], M) :- (V = shown,!; S = m; S = n), N = S; N = M.
markAroundREx(Y, [[shown,S]|T], [[shown,S]|RT], M) :-
    Y > -1, Y2 is Y-1,
    markAroundREx(Y2,T,RT,M).
markAroundREx(Y, [[V,S]|T], [[V,S]|RT], M) :-
    V \= shown, (S = m; S = n),
    Y > -1, Y2 is Y-1,
    markAroundREx(Y2, T, RT, M).
markAroundREx(Y, [[V,S]|T], [[V,N]|RT], M) :-
    Y > -1, V \= shown,
    (
        Y = 1, N = M;
        Y \= 1, N = S
    ),
    Y2 is Y-1,
    markAroundREx(Y2,T,RT,M).

% fillEdges(+G,-H)
% Finds a correct 'n'/'m' arrangement of all edge fields,
% provided such an arrangement exists
fillEdges(G,H) :-
    tryEdge(G,G1),
    infer(G1,G2),
    withoutOverflows(G2),
    (
        G \= G2, fillEdges(G2,H);
        G = G2, \+ unfilledEdges(G2), writeln('good'),H = G2
    ).

% unfilledEdges(+G)
% Succeeds iff G contains a non-'m', non-'n' edge
unfilledEdges([[]|T]) :- unfilledEdges(T).
unfilledEdges([[[V,S]|HT]|T]) :-
    V = edge, S \= m, S \= n,!;
    unfilledEdges([HT|T]).

/* -- Possibility search -- */


/* ++ Search and alter functions ++ */

% field(+X,+Y,+G,?F)
% Finds a field at position X,Y in G
field(X,Y,Grid,Field) :-
    nth0(X,Grid,Row),
    nth0(Y,Row,Field).

% reveal(+X,+Y,+G,-H)
% Sets H to a copy of G with point X,Y set as 'shown'
reveal(0,Y,[Old|T],[New|T]) :- revealR(Y,Old,New),!.
reveal(X,Y,[H|T],[H|R]) :- X1 is X-1, reveal(X1,Y,T,R).

revealR(0, [[_,S]|T], [[shown,S]|T]).
revealR(Y, [H|T], [H|R]) :- Y > 0, Y1 is Y-1, revealR(Y1,T,R).

% revealAll(+G,-H)
% Sets all fields in G to 'shown'
revealAll([], []).
revealAll([OH|OT],[RH|RT]) :- revealRow(OH,RH), revealAll(OT,RT).

revealRow([], []).
revealRow([[_,X]|T], [[shown,X]|T2]) :- revealRow(T, T2).

% updateAround(+X,+Y,+G,-H,+V)
% Sets visibility of neigbours of X,Y to 
% V, unless the neighbour is 'shown'
updateAround(_, _, [], [], _).
updateAround(-1, Y, [H|T], [R|T], V) :- updateAroundR(Y, H, R, V).
updateAround(X, Y, [H|T], [R|RT], V) :-
    (X = 0; X = 1),
    updateAroundR(Y, H, R, V),
    X2 is X-1,
    updateAround(X2, Y, T, RT, V).
updateAround(X, Y, [H|T], [H|RT], V) :-
    \+ diffOne(X,0),
    X2 is X-1,
    updateAround(X2, Y, T, RT, V).

updateAroundR(_, [], [], _).
updateAroundR(-1, [[OV,S]|T], [[NV,S]|T], V) :- newVisib(OV,V,NV).
updateAroundR(Y, [[OV,S]|T], [[NV,S]|RT], V) :-
    (Y = 0; Y = 1),
    newVisib(OV, V, NV),
    Y2 is Y-1,
    updateAroundR(Y2, T, RT, V).
updateAroundR(Y, [H|T], [H|RT], V) :-
    \+ diffOne(Y, 0),
    Y2 is Y-1,
    updateAroundR(Y2, T, RT, V).

newVisib(shown,_,shown).
newVisib(OV,NV,NV) :- OV \= shown.

% changeAround(+F,+X,+Y,+G,-H)
% Sets visibility of fields around X,Y, based on the type of F.
% F MUST be equal to G[X,Y]
changeAround([edge,_],_,_,G,G).
changeAround([deep,_],_,_,G,G).
changeAround([shown,x],_,_,G,G).
changeAround([shown,a],_,_,G,G).
changeAround([shown,0],X,Y,G,H) :- updateAround(X,Y,G,H,shown).
changeAround([shown,N],X,Y,G,H) :- N > 0, updateAround(X,Y,G,H,edge).

% updateGrid(+G,-H)
% Reveals around revealed 0s and sets correct 'edge's.
updateGrid([], []).
updateGrid([H|T], Res) :-
    length(H,WW), W is WW-1,
    length(T,L),
    updateGrid_(L,W,[H|T],G2),
    (
        [H|T] = G2,!, G2 = Res;
        updateGrid(G2, Res)
    ).

updateGrid_(X,_,G,G) :- X < 0.
updateGrid_(X,W,G,H) :- 
    X >= 0,
    updateGridR(X,W,G,G2),
    X2 is X-1,
    updateGrid_(X2,W,G2,H).
updateGridR(_,Y,G,G) :- Y < 0.
updateGridR(X,Y,G,H) :-
    Y >= 0,
    (
        field(X,Y,G,F),
        changeAround(F,X,Y,G,G2) ;
        G2 = G    
    ),
    Y2 is Y-1,
    updateGridR(X,Y2,G2,H).

% unsolved(+G)
% Succeeds iff G isn't solved
% G isn't solved if a nonrevealed nonmine remains
unsolved([]) :- false.
unsolved([[]|T]) :- unsolved(T).
unsolved([[[deep,_]|_]|_]).
unsolved([[[edge,S]|_]|_]) :- S \= x.
unsolved([[[shown,_]|HT]|T]) :-
    unsolved([HT|T]).
unsolved([[[edge,x]|HT]|T]) :-
    unsolved([HT|T]).

/* -- Search and alter functions -- */


/* ++ Grid Setup ++ */

% fillMines(+L,-G)
% Creates a mine grid of level L
fillMines(L,G) :-
    avgMineCount(L,A),
    diffStats(L,H,W,D),
    fillMines_(H,W,D,G1),
    mineCount(G1,N),
    (
        diffFive(N,A), G=G1,!;
        fillMines(L,G)
    ).

fillMines_(0,_,_,[]) :- !.
fillMines_(H,W,D,[Row|Grid]) :-
    fillMinesRow(W,D,Row),
    NewH is H-1,
    fillMines_(NewH,W,D,Grid).

fillMinesRow(0,_,[]) :- !.
fillMinesRow(W,D,[[deep,H]|T]) :-
    (
        random(0, D, 0), H = x,!;
        H = a
    ),
    W1 is W-1,
    fillMinesRow(W1,D,T).

% fillNumbers(+G,-H)
% Fills mine grid with adjacency numbers for nonmines
fillNumbers([], []).
fillNumbers(G, H) :- fillNumbers_(G, H, G, 0).

fillNumbers_([], [], _, _).
fillNumbers_([H|T], [H2|T2], G, X) :- 
    fillNumbersRow(X, 0, G, H, H2), 
    X2 is X+1, fillNumbers_(T, T2, G, X2).

fillNumbersRow(_, _, _, [], []).
fillNumbersRow(X, Y, G, [[V,O]|T], [[V,N]|T2]) :-
    (
        O=x, N=x,!; 
        minesAround(X,Y,G,N)
    ),
    Y2 is Y+1,
    fillNumbersRow(X, Y2, G, T, T2).

% safeReveal(+X,+Y,+G,-H)
% Reveals field at X,Y, changing it to non-mine if it is a mine.
% SHOULD NOT be used after fillNumbers, will break the grid
safeReveal(_,_,[],[]).
safeReveal(0,Y,[H|T], [R|T]) :- safeRevealR(Y,H,R).
safeReveal(X,Y,[H|T], [H|RT]) :-
    X > 0, X2 is X-1,
    safeReveal(X2, Y, T, RT).
safeRevealR(_,[],[]).
safeRevealR(0,[_|T],[[shown,a]|T]).
safeRevealR(Y,[H|T],[H|RT]) :-
    Y > 0, Y2 is Y-1,
    safeRevealR(Y2,T,RT).

% size(+G, ?L, ?W)
% Finds the dimensions of G ... L = Height, W = Width
size([H|T], L, W) :-
    length([H|T],L),
    length(H,W).

% diffStats(+Level,-L,-W,-P)
% Finds grid info for Level
diffStats(1,10,10,9).
diffStats(2,16,16,7).
diffStats(3,30,16,5).

avgMineCount(1,11).
avgMineCount(2,37).
avgMineCount(3,96).

/* --Grid setup-- */


/* ++ Counting functions ++ */

% around(+X,+Y,+G,-A)
% returns a grid of fields around the point X,Y of G, excluding X,Y
around(_,_,[],[]).
around(-1, Y, [H|_], [R]) :-
    aroundR(Y, H, R).
around(X, Y, [H|T], [R|RT]) :-
    (
        X = 1, aroundR(Y, H, R);
        X = 0, aroundREx(Y, H, R)
    ), 
    X2 is X-1,
    around(X2, Y, T, RT).
around(X, Y, [_|T], RT) :-
    \+ diffOne(X, 0),
    X2 is X-1, 
    around(X2, Y, T, RT).

aroundR(_, [], []).
aroundR(-1, [H|_], [H]).
aroundR(Y, [H|T], [H|RT]) :-
    (Y = 0; Y = 1),
    Y2 is Y-1,
    aroundR(Y2, T, RT).
aroundR(Y, [_|T], RT) :-
    \+ diffOne(Y, 0),
    Y2 is Y-1,
    aroundR(Y2, T, RT).

aroundREx(_, [], []).
aroundREx(-1, [H|_], [H]).
aroundREx(1, [H|T], [H|RT]) :-
    aroundREx(0, T, RT).
aroundREx(Y, [_|T], RT) :-
    (Y = 0; \+ diffOne(Y,0)),
    Y2 is Y-1,
    aroundREx(Y2, T, RT).


diffOne(A,A).
diffOne(A,B) :- B is A+1; B is A-1.
diffFive(A,B) :-
    N is B-A,
    N < 6, N > -6.

% minesAround(+X,+Y,+G,?N)
% Counts the number of 'x' mines around point X,Y
minesAround(X,Y,Grid,Res) :-
    around(X,Y,Grid,Around),
    mineCount(Around,Res).

mineCount([], 0).
mineCount([H|T], N) :-
    mineCountR(H, N1),
    mineCount(T, N2),
    N is N1+N2.

mineCountR([], 0).
mineCountR([[_,S]|T],R) :- 
    (S = x; S = m), mineCountR(T,X), R is X+1,!;
    mineCountR(T,R).

% coveredAround(+X,+Y,+G,?N)
% Counts the number of covered fields around X,Y in G
% A field is covered iff it isn't 'shown or 'n'
coveredAround(X,Y,Grid,Res) :-
    around(X,Y,Grid,Around),
    coveredCount(Around,Res).

coveredCount([],0).
coveredCount([[]|T], N) :- coveredCount(T,N).
coveredCount([[[shown,_]|HT]|T], N) :-    
    coveredCount([HT|T],N).
coveredCount([[[V,n]|HT]|T], N) :-
    V \= shown,
    coveredCount([HT|T], N).
coveredCount([[[V,S]|HT]|T], N) :-
    V \= shown, S \= n,
    coveredCount([HT|T], TN),
    N is TN+1.

% sureMinesAround(+X,+Y,+G,?N)
% Counts the number of 'm' mines around X,Y in G
sureMinesAround(X,Y,Grid,Res) :-
    around(X,Y,Grid,Around),
    sureMineCount(Around,Res).

sureMineCount([],0).
sureMineCount([[]|T], N) :- sureMineCount(T,N).
sureMineCount([[[_,S]|HT]|T], N) :-
    sureMineCount([HT|T], TN),
    (
        S = m, N is TN+1;
        S \= m, N = TN
    ).
/* -- Counting functions -- */


/* ++ Display functions ++ */
% The following sections are self-explanatory

showGrid([H|T]) :- nl, length(H, L), showTopHeader(L), showGrid_([H|T], 0), showBottomHeader(L), nl.

showTopHeader(L) :-
    showHeader(0,L),
    showHeaderLine(L).

showBottomHeader(L) :-
    showHeaderLine(L),
    showHeader(0,L).

showHeader(X,L) :- tab(3), showHeader_(X,L).

showHeader_(X,L) :- 
    X >= L, writeln('|'),!.
showHeader_(X,L) :-
    write('|'), Code is 65+X, char_code(Char, Code), write(Char), 
    N is X+1, showHeader_(N,L).

showHeaderLine(X) :- tab(3), showHeaderLine_(X).

showHeaderLine_(0) :- writeln('-'),!.
showHeaderLine_(X) :- 
    write('--'), N is X-1, showHeaderLine_(N).

showGrid_([], _).
showGrid_([H|T], N) :- 
    showLeftRowNumber(N), 
    showRow(H), 
    showRightRowNumber(N),
    Next is N+1,
    showGrid_(T, Next).

showLeftRowNumber(N) :-
    (N < 10, tab(1),!; true),
    write(N), write('|').

showRightRowNumber(N) :-
    write('|'), writeln(N).


showRow([]) :- write('|').
showRow([H|T]) :-
    showField(H),
    showRow(T).

showField(F) :- write('|'), showField_(F).

showField_([shown,x]) :- write('X'),!.
%showField_([_,m]) :- write(m),!.
%showField_([_,n]) :- write(n),!.
%showField_([_,a]) :- write(a),!.
showField_([shown,S]) :- S \= x, write(S).
%showField_([edge,_]) :- write(e).
showField_([V,_]) :- V \= shown, tab(1).

welcomeScreen :-
    nl,writeln('WELCOME TO MINE DEVIL!'), nl.

/* -- Display functions -- */


/* ++ User input ++ */

getCoords(X,Y) :-
    getInput(L, 'Row'),
    L = [X], number(X),
    getInput(L2, 'Column'),
    L2 = [C],  cToY(C,Y) ;
    writeln('Invalid input!'),
    getCoords(X,Y).

validCoords(X,Y,L,W) :-
    X >= 0, Y >= 0, X < L, Y < W.

getInput(X,T) :-
    write(T), writeln(':'),
    readln(X).

cToY(C,Y) :-
    to_upper(C,U),
    Y is U-65.

/* -- User input -- */

/* ++ Testing grids ++ */
testlist([0,1,2,3,4,5,6,7,8,9]).
testArray([[0,1,2,3,4],[5,6,7,8,9],[10,11,12,13,14],[15,16,17,18,19],[20,21,22,23,24]]).
testGrid([[[deep,0],[deep,1],[deep,2],[deep,3],[deep,4]],
    [[deep,5],[deep,6],[deep,7],[deep,x],[deep,9]],
    [[deep,0],[deep,x],[deep,2],[deep,3],[deep,x]],
    [[deep,5],[deep,6],[deep,x],[deep,8],[deep,9]],
    [[deep,0],[deep,1],[deep,2],[deep,3],[deep,x]]]).
testRealGrid(
    [[[shown,1],[shown,2],[edge,2]],
     [[shown,1],[edge,x],[edge,x]],
     [[edge,1],[edge,2],[deep,2]]]).
testSmallGrid(
    [[[edge,1],[edge,x]],
     [[shown,1],[edge,1]]
    ]).
testBigGrid(
    [[[deep,1],[deep,1],[deep,0],[deep,1],[deep,x],[deep,1],[deep,0],[deep,0]],
    [[deep,x],[deep,1],[deep,0],[deep,1],[deep,1],[deep,2],[deep,2],[deep,2]],
    [[deep,1],[deep,1],[deep,0],[deep,0],[deep,0],[deep,2],[deep,x],[deep,x]],
    [[deep,0],[deep,0],[deep,0],[deep,0],[deep,0],[deep,2],[deep,x],[deep,3]],
    [[deep,0],[deep,0],[deep,0],[deep,0],[deep,0],[deep,1],[deep,1],[deep,1]],
    [[deep,1],[deep,1],[deep,1],[deep,0],[deep,0],[deep,0],[deep,0],[deep,0]],
    [[deep,1],[deep,x],[deep,1],[deep,0],[deep,0],[deep,0],[deep,0],[deep,0]],
    [[deep,1],[deep,1],[deep,1],[deep,0],[deep,0],[deep,0],[deep,0],[deep,0]]]    ).

testMineGrid(
    [[[deep,a],[deep,x],[deep,a],[deep,a],[deep,a],[deep,a]],
    [[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a]],
    [[deep,a],[deep,x],[deep,a],[deep,a],[deep,a],[deep,a]],
    [[deep,a],[deep,x],[deep,a],[deep,a],[deep,a],[deep,a]],
    [[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,x]],
    [[deep,a],[deep,x],[deep,a],[deep,a],[deep,x],[deep,a]]]
    ).

testInfer(
    [[[deep,x],[deep,x],[deep,x],[deep,x],[deep,x],[deep,x]],
     [[deep,2],[deep,3],[deep,3],[deep,3],[deep,3],[deep,2]],
     [[deep,1],[deep,1],[deep,2],[deep,2],[deep,2],[deep,1]],
     [[shown,1],[edge,x],[edge,1],[edge,x],[edge,x],[edge,1]],
     [[shown,1],[shown,1],[shown,2],[shown,2],[shown,2],[edge,1]],
     [[shown,0],[shown,0],[shown,0],[shown,0],[shown,0],[shown,0]]
    ]).
testInfer2(
    [[[edge,x],[shown,1],[edge,0]],
     [[shown,1],[shown,1],[edge,0]],
     [[shown,1],[shown,2],[shown,1]],
     [[edge,x],[edge,2],[edge,x]]

    ]).

testLevel3([[[deep,x],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,x],[deep,x],[deep,a],[deep,a],[deep,a],[deep,x],[deep,a],[deep,a],[deep,a],[deep,a]],[[deep,a],[deep,a],[deep,a],[deep,x],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,x],[deep,x],[deep,a],[deep,a],[deep,x]],[[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,x],[deep,a],[deep,a],[deep,a],[deep,a],[deep,x]],[[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,x],[deep,x],[deep,a],[deep,a],[deep,x],[deep,a],[deep,x]],[[deep,a],[deep,a],[deep,x],[deep,a],[deep,a],[deep,a],[deep,a],[deep,x],[deep,x],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a]],[[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,x],[deep,a],[deep,a],[deep,a],[deep,a],[deep,x],[deep,a],[deep,a],[deep,a],[deep,a]],[[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,x],[deep,a],[deep,a],[deep,x],[deep,a],[deep,a],[deep,x]],[[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,x],[deep,a],[deep,a],[deep,a],[deep,a],[deep,x],[deep,a],[deep,x],[deep,a],[deep,x],[deep,a]],[[deep,a],[deep,x],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,x],[deep,a],[deep,x],[deep,a],[deep,a],[deep,a]],[[deep,x],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,x],[deep,x],[deep,a],[deep,a],[deep,x]],[[deep,a],[deep,a],[deep,x],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,x],[deep,a],[deep,a],[deep,x],[deep,a],[deep,a],[deep,a],[deep,a]],[[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,x],[deep,x],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,x]],[[deep,a],[deep,x],[deep,a],[deep,a],[deep,a],[deep,a],[deep,x],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,x],[deep,a],[deep,a]],[[deep,a],[deep,x],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a]],[[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,x],[deep,a],[deep,a],[deep,a],[deep,a],[deep,x],[deep,a],[deep,a],[deep,a],[deep,a]],[[deep,a],[deep,a],[deep,a],[deep,a],[deep,x],[deep,a],[deep,x],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a]],[[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,x],[deep,a],[deep,a],[deep,a],[deep,x],[deep,a]],[[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,x],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,x],[deep,a],[deep,a]],[[deep,x],[deep,x],[deep,x],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,x],[deep,x],[deep,x],[deep,a],[deep,a],[deep,x],[deep,a],[deep,a]],[[deep,a],[deep,a],[deep,a],[deep,x],[deep,a],[deep,x],[deep,a],[deep,a],[deep,a],[deep,a],[deep,x],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a]],[[deep,a],[deep,a],[deep,a],[deep,x],[deep,a],[deep,a],[deep,a],[deep,a],[deep,x],[deep,a],[deep,x],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a]],[[deep,x],[deep,x],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,x],[deep,a],[deep,x],[deep,a],[deep,a],[deep,a],[deep,a]],[[deep,x],[deep,a],[deep,x],[deep,a],[deep,a],[deep,a],[deep,a],[deep,x],[deep,x],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,x]],[[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,x],[deep,a],[deep,a],[deep,a],[deep,x],[deep,a]],[[deep,a],[deep,a],[deep,a],[deep,a],[deep,x],[deep,x],[deep,a],[deep,x],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a]],[[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,x],[deep,a],[deep,x],[deep,a],[deep,x],[deep,a],[deep,a],[deep,a],[deep,a]],[[deep,x],[deep,x],[deep,a],[deep,x],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,x],[deep,a],[deep,a],[deep,a],[deep,a]],[[deep,x],[deep,a],[deep,a],[deep,a],[deep,x],[deep,a],[deep,a],[deep,a],[deep,a],[deep,x],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,x]],[[deep,a],[deep,x],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,x],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,x],[deep,a]],[[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,x],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a]]]).
testLevel1([[[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a]],[[deep,a],[deep,a],[deep,a],[deep,x],[deep,a],[deep,a],[deep,x],[deep,a],[deep,a],[deep,x]],[[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,x],[deep,a]],[[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a]],[[deep,x],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a]],[[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a]],[[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,x]],[[deep,a],[deep,a],[deep,x],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a]],[[deep,a],[deep,a],[deep,x],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a]],[[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a],[deep,a]]]).

debugGrid(G,T) :-
    showGrid(G),!,
    writeln(T),
    readln(_).
/* -- Testing grids -- */





