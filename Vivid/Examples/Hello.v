init() {
	files = io.get_folder_files('/home/jolehto/Lataukset', true)

	loop file in files {
		println(file.fullname)
	}

	io.write_file('/home/jolehto/Lataukset/test.txt', String('Haloo kuuluuko'))

	if io.read_file('/home/jolehto/Lataukset/test.txt') has data {
		println(String.from(data.data, data.count))
	}

	println(io.exists('/home/jolehto/Lataukset/test.txt'))
	println(io.exists('/home/jolehto/Lataukset/boo.txt'))
	
	println(io.exists('/home/jolehto/Lataukset/Viper4Linux'))

	println(io.is_folder('/home/jolehto/Lataukset/Viper4Linux'))
	println(io.is_folder('/home/jolehto/Lataukset/test.txt'))

	println(io.size('/home/jolehto/Lataukset/test.txt'))
	println(io.size('/home/jolehto/Lataukset/Viper4Linux'))

	=> 0
}