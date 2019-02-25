muz(ja).
muz(otec).
muz(synV).
muz(synD).

zena(vdova).
zena(dcera).

rodic(otec, ja).
rodic(vdova,dcera).
rodic(vdova,synV).
rodic(dcera,synD).

manzele(vdova, ja).
manzele(otec,dcera).

manzeleSym(X,Y) :- manzele(X,Y); manzele(Y,X).
fullRodic(X,Y) :- rodic(X,Y).
fullRodic(X,Y) :- rodic(Z,Y), manzeleSym(X,Z).

zet(X,Y) :- fullRodic(Y,Z), manzeleSym(X,Z), muz(X).
matka(X,Y) :- fullRodic(X,Y), zena(X).
otec(X,Y) :- fullRodic(X,Y), muz(X).
bratr(X,Y) :- fullRodic(Z,X), fullRodic(Z,Y), muz(X).
sestra(X,Y) :- fullRodic(Z,X), fullRodic(Z,Y), zena(X).
svagr(X,Y) :- bratr(X,Z), manzeleSym(Y,Z).   % bratr manzela/manzelky
svagr(X,Y) :- sestra(Z,Y), manzeleSym(X,Z).  % manzel sestry
stryc(X,Y) :- fullRodic(Z,Y), bratr(X,Z).    % bratr rodice
prarodic(X,Y) :- fullRodic(X,Z), fullRodic(Z,Y).
babicka(X,Y) :- prarodic(X,Y), zena(X).
dedecek(X,Y) :- prarodic(X,Y), muz(X).
vnuk(X,Y) :- prarodic(Y,X), muz(X).
vnucka(X,Y) :- prarodic(Y,X), zena(X).

