import System.IO ()

main :: IO ()
main = do
    putStrLn "Cesta k souboru:"
    path <- getLine
    putStrLn "read/write?"
    op <- getLine
    process path op

process :: FilePath -> String -> IO ()
process path "read" = do
    file <- readFile path
    putStrLn file

process path "write" = do
    putStrLn "Novy obsah:"
    new <- getLine
    writeFile path new

process _ _ = do
    putStrLn "Neplatna operace. Zadejte 'read'/'write'."