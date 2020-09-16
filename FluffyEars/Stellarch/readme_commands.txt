Module: Filter System
	!filter new/add <regex string>			Adds a new regex string to the filter system.				!filter add b[aeiou]nny
	!filter remove/delete <regex string>	Removes an existing regex string from the filter system.	!filter remove b[aeiou]nny
	!filter list							Lists all regex strings in the filter system.
	
	!exclude new/add <exclude string>		Adds a non-regex string to phrase excludes.	!exclude add bannyen
	!exclude remove/delete <exclude string>	Removes an entry from the phrase excludes.	!exclude remove bannyen
	!exclude list							Lists all phrases currently being excluded.

