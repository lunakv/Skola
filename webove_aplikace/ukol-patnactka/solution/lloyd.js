/*
 * Your code goes here.
 * Do not forget that this code may be executed before the whole HTML is parsed
 * (i.e., the DOM structure may not be yet complete).
 */

/*
 * You may use the following initial position if you do not implement randomization.
 *  3  11   2   5
 *  1  13   6   8
 *  4   9      10
 * 14  12   7  15
 */

function fillBoard(board) {
    let tiles = [1,2,3,4,5,6,7,8,9,10,11,12,undefined,13,14,15];
    for (let i = 0; i < 16; i++) {
        if (tiles[i] == undefined)
            board.layout[i] = undefined;
        else
            board.layout[i] = document.getElementById("game-tile-"+tiles[i]);
    }
}

function setHandlers(board) {
    board.layout.forEach(tile => {
        if (!tile) return;
        tile.onclick = () => {
            board.move(tile);
            refresh(board, tile);
            if (board.isSolved())
            {
                markSolved(board);
            }
            else 
                markUnsolved(board);
        }
    })
}

function refresh(board, tile) {
    let pos = board.getPosition(tile);
    if (pos == -1) return;
    tile.style.top = (20 * Math.floor(pos/4)) + "vmin";
    tile.style.left = (20 * (pos % 4)) + "vmin";
}

function refreshBoard(board) {
    board.layout.forEach(tile => {if (tile) refresh(board, tile);});
}

function markSolved(board) {
    // window.alert("SOLVED!!!!");
    board.layout.forEach(tile => {if (tile) tile.style.color = "green"});
}

function markUnsolved(board) {
    board.layout.forEach(tile => {if (tile) tile.style.color = "black"});
}

class Board {
    constructor() {
        this.layout = new Array(16);
        this.solved = false;
    }

    randomize() {
        for(let i = 0; i < 500; i++) {
            this.move(this.randomMovable());
        }
    }

    randomMovable() {
        let space = this.getPosition(undefined);
        let candidates = [];
        if (space % 4 > 0) candidates.push(-1);
        if (space % 4 < 3) candidates.push(+1);
        if (Math.floor(space/4) > 0) candidates.push(-4);
        if (Math.floor(space/4) < 3) candidates.push(+4);
        let res = candidates[Math.floor(Math.random() * candidates.length)];
        return this.layout[space+res];      
    }

    isSolved() {
        for (let i = 0; i < 15; i++)
            if (!this.layout[i] || this.layout[i].innerHTML != i+1)
                return false;
        return true;
    }

    getPosition(tile) {
        for (let i = 0; i < 16; i++) {
            if (this.layout[i] == tile)
                return i;
        }
        return -1;
    }

    spaceDir(tile) {
        let pos = this.getPosition(tile);
        if (pos == -1) return null;
        let space = this.getPosition(undefined);

        if (pos % 4 < 3 && space == pos+1) return +1;
        if (pos % 4 > 0 && space == pos-1) return -1;
        if (Math.floor(pos/4) < 3 && space == pos+4) return +4;
        if (Math.floor(pos/4) > 0 && space == pos-4) return -4;
        return null;
    }

    move(tile) {
        let pos = this.getPosition(tile);
        let shift = this.spaceDir(tile);
        if (!shift) return;
        console.log("Pos: "+pos);
        console.log("Shift: "+shift);
        this.layout[pos+shift] = tile;
        this.layout[pos] = undefined;
    }
}

var board = new Board();

window.addEventListener("DOMContentLoaded", function() {
    fillBoard(board);
    board.randomize();
    refreshBoard(board);
    setHandlers(board);
});