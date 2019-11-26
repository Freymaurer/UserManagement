module Client.AuxFunctions

open System

/// function for similarity measurements
let sorensenCoefficent (str1:string) (str2:string) =
    let charSeq1 = str1.ToCharArray()
    let charSeq2 = str2.ToCharArray()
    let startLength1 = str1.Length
    let startLength2 = str2.Length
    let mutable str1Mut = str1
    let mutable str2Mut = str2
    let numberOfCommonSpecies1 =
        charSeq1
        |> Array.map (fun x -> (str2Mut.IndexOf x)
                               |> fun x -> if x < 0
                                           then str2Mut <- str2Mut
                                           else str2Mut <- str2Mut.Remove(x,1)
                     )
    let numberOfCommonSpecies2 =
        charSeq2
        |> Array.map (fun x -> (str1Mut.IndexOf x)
                               |> fun x -> if x < 0
                                           then str1Mut <- str1Mut
                                           else str1Mut <- str1Mut.Remove(x,1)
                     )
    let numberOfSpeciesCommon =
        (startLength2 - str2Mut.Length,startLength1 - str1Mut.Length)
        |> fun (x,y) -> if x <> y then failwith "unknown case"
                        else float x
    let numberOfSpeciesSpecificToFst =
        float startLength1 - numberOfSpeciesCommon
    let numberOfSpeciesSpecificToSnd =
        float startLength2 - numberOfSpeciesCommon
    (2. * numberOfSpeciesCommon)/((2. * numberOfSpeciesCommon) + numberOfSpeciesSpecificToFst + numberOfSpeciesSpecificToSnd)

let checkSeparatedHit (input:string) (searchStr:string) =
    let foundInd = input.IndexOf(searchStr)
    let followCharInd = foundInd + searchStr.Length
    let precedingCharInd = foundInd - 1
    if precedingCharInd < 0 && followCharInd > input.Length - 1
    then Some 2, true
    elif precedingCharInd < 0 || followCharInd > input.Length - 1
    then Some 1, true
    else let followChar = input.[followCharInd]
         let precedingChar = input.[precedingCharInd]
         if (Char.IsDigit followChar |> not && Char.IsLetter followChar |> not) && (Char.IsDigit precedingChar |> not && Char.IsLetter precedingChar |> not)
         then Some 2, true
         elif (Char.IsDigit followChar |> not && Char.IsLetter followChar |> not) || (Char.IsDigit precedingChar |> not && Char.IsLetter precedingChar |> not)
         then Some 1, true
         else None, false

/// This function ranks the compareString by the searchString, with a higher number being a better hit (more similiar).
let rankCompareStringsBySearchString (searchString:string) (compareString:string) =
    let sString =
        compareString
        |> String.map (fun c -> Char.ToUpper c)
    let searchStringUpper =
        searchString |> String.map (fun c -> Char.ToUpper c)
    if sString.Contains searchStringUpper
    then let separatedHitBonus = checkSeparatedHit sString searchStringUpper |> fun (x,y) -> if y = true then match x.Value with | 1 -> 1.25 | 2 -> 1.5 | _ -> 1. else 1.
         (sorensenCoefficent searchStringUpper sString) * 2. * separatedHitBonus, compareString
    else (sorensenCoefficent searchStringUpper sString), compareString