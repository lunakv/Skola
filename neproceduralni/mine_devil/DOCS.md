# Mine Devil

## Usage
1. Download this folder
2. Run `mine_devil.sh`

## Game state representation
- The mine grid is a 2D list of fields
- A field is a 2 element list `[V,S]`
- `V` is the visibility state of a field. Possible values:
    - `shown`, for revealed fields,
    - `edge`, for fields adjacent to at least one `shown` field,
    - `deep`, for all other fields.
- `S` is the mine status of a field. Possible values:
    - `a` == temporary initialization state, should be replaced by an appropriate value,
    - `x` == the initial setup set this field as a mine,
    - 0-8 == the initial setup set this field as non-mine, displaying the number of adjacent `x` mines,
    - `m` == based only on all `shown`, `m`, and `n` fields, a search determined this field MUST be a mine,
    - `n` == based only on all `shown`, `m`, and `n` fields, a search determined this field CANNOT be a mine.
- By their nature, `m` and `n` fields can only be `edge` visible.
- Every `deep` field is a possible mine
- An `edge` field is a possible mine if there exists a correct `m`/`n` edge arrangement such that the field is `m`.
    - An `m`/`n` edge arrangement assigns either `m` or `n` to every `edge` field
    - An `m`/`n` edge arrangement is correct iff every [shown,N] field neighbors exactly N `m` edges
- The game is lost when a player reveals a possible mine
- The game is won when only `x` fields are not revealed

## Game loop
1. Get a field as input from the user
2. Search for a valid mine layout where that field is a mine
3. If such a layout is found, the field is deemed as a possible mine and the game is over
4. Otherwise, reveal that field, update the visibility state of its neighbors and start again.

### Mine / non-mine inference
The mine search algorithm operates by applying two simple rules:

1. If a `[shown,N]` field has exactly N unsafe neighbours, all these neighbours must be sure mines (`m`)
  - a neighbour is unsafe unless it's `shown` or `n`
2. If a `[shown,N]` field has exactly N neighbours that are sure (`m`) mines, all of its other neighbours are sure non-mines (`n`)

Since applying one of these rules may enable the application of the other, they are applied repeatedly until the output is identical to the input.

### Searching for valid mine layouts
Any field that is either `deep` and/or `x` is always assumed as part of some valid mine layout. No `shown` field is part of any valid mine layout. This restricts the search space to just `edge` fields.

In order to search for valid mine layouts including field F as mine, we first mark it as `m`. Then, we find all sure mines/non-mines resulting from this using the inference described above.

If some `edge` fields are still undecided, we choose one and mark it as `n`. Then, we repeat the search until all edges are filled.

If this choice or its inferred results produce an impossible state (a field has too many `m` neighbours / not enough free places for `m` neighbours), we roll back to the last made choice and change that field to `m` instead. Then we repeat the search. If we still reach an impossible state, we move to the choice made one level above.

If it's possible for the selected field to be a mine, we'll find at least one correct `m`/`n` edge arrangement with this search. Conversely, it isn't possible for the field to be a mine iff any combination of choices made in the search leads to an impossible state.

By making the inference step after every guess, we significantly reduce the search time, since we don't have to make a binary choice for every single edge field. Such a simple approach would be especially time consuming on larger boards with multiple disjoint edge areas.

 
    

