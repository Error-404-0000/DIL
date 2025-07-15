


@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@USING .PY JUST FOR THE HIGHLIGHT ---- THIS IS NOT PYTHON   @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@



class Book:
    Title: "Unknown Title";
    Author: "Unknown Author";
    ISBN: 0 as int;
    CopiesAvailable: 0 ;
class:end;

class Member_Info:
    MemberId: 0 as int;
    Name: "Anonymous";
    Email: "unknown@example.com";

class:end;

class Library:
    Members: 0; // Empty array to hold Member_Info objects
    Books: 0;   // Empty array to hold Book objects
    LibraryType: "Public" $overwrite$; // Overwritten type
    Established: 1900 as int;
class:end;

let Library = Library:new;

// Add data dynamically
let Member1 = {
    MemberId: 1,
    Name: "John Doe",
    Email: "john.doe@example.com"
} as map;

let Member2 = {
    MemberId: 2,
    Name: "Jane Smith",
    Email: "jane.smith@example.com"
} as map;
Library->Established = 194500000000000000;
Library->LibraryType = "Private";
Get Library->LibraryType;
Get Library->Established;
// Access data
Library = Library:new;
Library->Members = [Member1, Member2] as array;

let Book1 = {
    Title: "1984",
    Author: "George Orwell",
    ISBN: 1234567890,

    CopiesAvailable: 10
} as map;

let Book2 = {
    Title: "Brave New World",
    Author: "Aldous Huxley",
    ISBN: 987654321,
    CopiesAvailable: 5
} as map;

Library->Books = [Book1, Book2] as array;

Get Library->LibraryType;
Get Library->Established;
Get Library->Books[0]->Title;
Get Library->Books[0]->Author;
Get Library->Books[0]->ISBN;
Get Library->Books[0]->CopiesAvailable;
Get Library->Members[0]->Name;
Get Library->Members[0]->Email;

// Map with nested data
let libraryConfig = {
    address: "123 Library St",
    floors: 3,
    sections: {
        fiction: {
            books: ["Book1", "Book2"],
            staff: 5
        },
        nonFiction: {
            books: ["Book3", "Book4"],
            staff: 3
        }
    }
} as map;

Get libraryConfig->address;
Get libraryConfig->sections->fiction->books[1]; // "Book2"
Get libraryConfig->sections->nonFiction->staff; // 3

// Update properties
Library->LibraryType = "Private";
Get Library->LibraryType; // "Private"

Library->Books[1]->CopiesAvailable = 8;
Get Library->Books[1]->CopiesAvailable; // 8
