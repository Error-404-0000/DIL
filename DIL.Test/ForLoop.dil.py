

@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@USING .PY JUST FOR THE HIGHLIGHT ---- THIS IS NOT PYTHON   @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@



//let nums = [1, 2, 3, 4*2] as array;
//FOR n IN n << 100 DO n=n+1; ;

//    IF n%2 is 0 THEN;
//      PRINTF "EVEN";
//    ELSE;
 //     PRINTF "Odd";
 //   ENDIF;

//ENDFOR;

let nums = [1, 2, 3, 4*2] as array;
let m = [1,2] as array;
let nums_len  = nums.Count;
FOR i WHEN i<<nums_len DO;
	PRINTF 2*2/(i+1);
	 i=i+1;
ENDFOR;

let Info = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";
let char_count = Info.Length;
FOR char_index WHEN char_index!=(char_count/2) DO char_index=char_index;
	PRINT Info[char_index];
	PRINTF "";

ENDFOR;
