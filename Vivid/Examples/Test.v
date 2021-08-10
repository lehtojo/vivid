MAPSIZE = 25
init(){
	Map = Sheet<char>(MAPSIZE,MAPSIZE)
	loop(x = 0, x < MAPSIZE,x++){
		loop(y = 0, y < MAPSIZE,y++){
			if(x == 0 or y == 0 or x == MAPSIZE-1 or y == MAPSIZE-1){
					Map[x,y] = `#`
			}
			else{
					Map[x,y] = `_`
			}
		}
	}
	loop{
		Map = Main.tick(Map)
	}
}
gotoxy(x : normal,y : normal)    
{
	#printf('%c[%d;%df',0x1B,y,x);
	print(String('\x1B[')+to_string(y + 1)+';'+to_string(x + 1)+'f')
}
Main{
	static tick(map : Sheet<char>){
		map = formap(map, (x,y,mapIn)->{
			gotoxy(x,y)
			print(String(mapIn[x,y]))
			=> mapIn[x,y]
		})
		=> map
	}

	static formap(map , do : (normal, normal, Sheet<char>) -> char){
		returned = Sheet<char>(MAPSIZE,MAPSIZE)
		loop(x = 0, x < MAPSIZE,x++){
			loop(y = 0, y < MAPSIZE,y++){

					returned[x,y] = do(x as normal,y as normal, map as Sheet<char>)

			}
		}
		=> returned
	}
}