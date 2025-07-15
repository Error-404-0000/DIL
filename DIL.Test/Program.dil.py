






@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@USING .PY JUST FOR THE HIGHLIGHT ---- THIS IS NOT PYTHON   @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
class Pet:
	name:"no name";
	id:0;
    Keys:[2,2] as array;
class:end;

class User_Info1:
	UserId : 0 as int;
	Email : "demon@gmail.com";
	Password : "Pass123";
class:end;

class User:
	User_Info:User_Info1 as class;
	User_Pet:Pet as class;
	Keys : [(2*80/4),1,2] as array;
    Type : "any" $overwrite$;//why overwrite? because by default on class creation the Type is auto added with value being  <ClassName>
class:end;

let User = User:new;

Get User->User_Info->Type;
Get User->Type;
Get User->User_Info->UserId;
Get User->User_Info->Email;
Get User->User_Info->Password;
Get User->User_Pet->name;
Get User->User_Pet->id;

let m =     {
    id:1,
    games:
        [[1,2,3],[2,3]],
    logs:{
         newlog:{
             id:1333,
             log:"HII"
          }
     }
} as map;

Get User->Keys[0];
Get m->id;
Get m->games[0][1];
Get m->logs->newlog->log[0];//H

User = Pet:new;
Get User->Type;
User->Keys[0] = 12000000;
Get User->Keys[0];

IF User->Type is "Pet" THEN;
    PRINT YES IT IS A PET;
ELSE;
    PRINT NOPE IT IS NOT A PART;
ENDIF;
let number = 20;

//if true set value to right value
IF number >>= 10 THEN;
    PRINTF "VALUE EDITED TO 10";
ENDIF;
let age = 20;
let person_type = "" ;
let profile = {} as map;


IF age >= 18 THEN;
    person_type = "Adult" as string;
    profile = {
        status: "verified",
        can_vote: true,
        can_drive: true
    } as map;
ELSE;
    person_type = "Minor";
    profile = {
        status: "restricted",
        can_vote: false,
        can_drive: false
    } as map;
ENDIF;

// Consistent formatted output
PRINTF "___________Person Info__________";
PRINT "Age: ";
PRINTF age;

PRINT "Type: ";
PRINTF person_type;

PRINT "Status: ";
PRINTF profile->status;

PRINT "Can Vote: ";
PRINTF profile->can_vote;

PRINT "Can Drive: ";
PRINTF profile->can_drive;

PRINTF "----------------------";

