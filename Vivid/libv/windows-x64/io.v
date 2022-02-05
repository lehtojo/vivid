import init(): large

export internal_init(root: link) {
	#internal.allocator.initialize()

	# Call the actual init function here
	init()
}

namespace io

namespace internal {
	constant INFINITE = 4294967295 # 0xFFFFFFFF

	constant GENERIC_WRITE = 1073741824
	constant GENERIC_READ = 2147483648
	
	constant FILE_SHARE_NONE = 0
	constant FILE_SHARE_READ = 1
	constant FILE_SHARE_WRITE = 2
	constant FILE_SHARE_DELETE = 4

	constant CREATE_NEW = 1
	constant CREATE_ALWAYS = 2
	constant OPEN_EXISTING = 3
	constant OPEN_ALWAYS = 4
	constant TRUNCATE_EXISTING = 4

	constant FILE_ATTRIBUTE_NORMAL = 128
	constant FILE_ATTRIBUTE_FOLDER = 16

	import 'C' GetLastError(): large

	import 'C' FindFirstFileA(filename: link, iterator: FileIterator): link
	import 'C' FindNextFileA(handle: link, iterator: FileIterator): link

	import 'C' CreateFileA(filename: link, access: normal, share_mode: normal, security_attributes: link, creation_disposition: normal, flags_and_attributes: normal, template: link): link
	import 'C' GetFileSizeEx(handle: link, size: link<large>): bool
	import 'C' WriteFile(handle: link, buffer: link, size: large, written: link<large>, overlapped: link): bool
	import 'C' ReadFile(handle: link, buffer: link, size: large, read: link<large>, overlapped: link): bool
	import 'C' CloseHandle(handle: link): bool

	import 'C' GetFileAttributesA(path: link): u32

	constant MAXIMUM_PATH_LENGTH = 260

	FileIterator {
		attributes: normal
		creation_time: large
		last_access_time: large
		last_write_time: large
		file_size: large
		reserved: large
		filename: char[MAXIMUM_PATH_LENGTH]
		alternate_filename: char[14]
		file_type: normal
		creator_type: normal
		finder_flags: small
	}

	constant ERROR_INSUFFICIENT_BUFFER = 122
	constant MINIMUM_PROCESS_FILENAME_LENGTH = 50

	import 'C' GetEnvironmentVariableA(name: link, buffer: link, size: large): large
	import 'C' GetModuleFileNameA(module: link, buffer: link, size: large): large

	import 'C' GetCommandLineA(): link

	plain StartupInformation {
		size: normal
		reserved_1: link
		desktop: link
		title: link
		x: normal
		y: normal
		width: normal
		height: normal
		console_width: normal
		console_height: normal
		fill_attributes: normal
		flags: normal
		show_window: small
		reserved_2: small
		reserved_3: link
		standard_input_handle: link
		standard_output_handle: link
		standard_error_handle: link
	}

	plain ProcessInformation {
		handle: link
		thread: link
		pid: normal
		tid: normal
	}

	import 'C' CreateProcessA(name: link, command_line: link, process_attributes: link, thread_attributes: link, inherit_handles: bool, creation_flags: large, environment: link, working_folder: link, startup_information: StartupInformation, process_information: ProcessInformation): bool

	constant PROCESS_ACCESS_SYNCHRONIZE = 1048576

	import 'C' OpenProcess(desired_access: normal, inherit_handles: bool, pid: normal): link
	import 'C' WaitForSingleObject(handle: link, milliseconds: normal): normal

	import 'C' GetCurrentDirectoryA(size: large, buffer: link): large
}

FolderItem {
	fullname: String
	is_folder: bool

	init(fullname: String, is_folder: bool) {
		this.fullname = fullname
		this.is_folder = is_folder
	}
}

# Summary: Returns all the filenames inside the specified folder
export get_folder_items(folder: link, all: bool) {
	=> get_folder_items(String(folder), all)
}

# Summary: Returns all the filenames inside the specified folder
export get_folder_items(folder: String, all: bool) {
	iterator = inline internal.FileIterator()

	filename: char[internal.MAXIMUM_PATH_LENGTH]
	
	# Ensure the folder ends with a separator
	if not folder.ends_with('/') and not folder.ends_with('\\') {
		folder = folder + '/'
	}

	filter = folder + '*'
	copy(filter.data(), internal.MAXIMUM_PATH_LENGTH, filename as link)

	file = internal.FindFirstFileA(filename as link, (iterator as link + 8) as internal.FileIterator)
	items = List<FolderItem>()

	# If the handle is none, return an empty list of files
	if file == none => items

	loop {
		name = String(iterator.filename as link)
		fullname = folder + name

		# Add the current value to items, if it does not have the folder flag
		if (iterator.attributes & internal.FILE_ATTRIBUTE_FOLDER) == 0 {
			items.add(FolderItem(fullname, false))
		}
		else all and not (name == '.' or name == '..') {
			items.add(FolderItem(fullname, true))
			items.add_range(get_folder_items(fullname, true))
		}
		
		# Try to get the next file, if it is none, it means all the files have been collected
		if internal.FindNextFileA(file, (iterator as link + 8) as internal.FileIterator) == 0 => items
	}
}

# Summary: Returns all the filenames inside the specified folder
export get_folder_files(folder: String, all: bool) {
	items = get_folder_items(folder, all)
	files = List<FolderItem>()

	loop item in items {
		if item.is_folder continue
		files.add(item)
	}

	=> files
}

# Summary: Returns all the filenames inside the specified folder
export get_folder_files(folder: link, all: bool) {
	=> get_folder_files(String(folder), all)
}

# Summary: Writes the specified text to the specified file
export write_file(filename: String, text: String) {
	=> write_file(filename.text, Array<byte>(text.text, text.length))
}

# Summary: Writes the specified text to the specified file
export write_file(filename: String, bytes: Array<byte>) {
	=> write_file(filename.text, bytes)
}

# Summary: Writes the specified text to the specified file
export write_file(filename: link, text: String) {
	=> write_file(filename, Array<byte>(text.text, text.length))
}

# Summary: Writes the specified byte array to the specified file
export write_file(filename: link, bytes: Array<byte>) {
	# Try to open the specified file
	file = internal.CreateFileA(filename, internal.GENERIC_WRITE, internal.FILE_SHARE_READ, none, internal.CREATE_ALWAYS, internal.FILE_ATTRIBUTE_NORMAL, none)
	if file == none => false

	# Write the specified byte array to the opened file
	written: large[1]
	result = internal.WriteFile(file, bytes.data, bytes.count, written as link<large>, none)

	# Finally, release the handle
	internal.CloseHandle(file)

	=> result
}

# Summary: Opens the specified file and returns its contents
export read_file(filename: String) => read_file(filename.text)

# Summary: Opens the specified file and returns its contents
export read_file(filename: link) {
	# Try to open the specified file
	file = internal.CreateFileA(filename, internal.GENERIC_READ, internal.FILE_SHARE_READ, none, internal.OPEN_ALWAYS, internal.FILE_ATTRIBUTE_NORMAL, none)
	if file == none => Optional<Array<byte>>()

	# Try to get the size of the opened file
	size: large[1]

	if internal.GetFileSizeEx(file, size as link<large>) == 0 {
		internal.CloseHandle(file)
		=> Optional<Array<byte>>()
	}
	
	buffer = Array<byte>(size[0])

	if internal.ReadFile(file, buffer.data, buffer.count, size as link<large>, none) == 0 {
		internal.CloseHandle(file)
		=> Optional<Array<byte>>()
	}
	
	# Finally, release the handle
	internal.CloseHandle(file)

	=> Optional<Array<byte>>(buffer)
}

# Summary: Returns whether the specified file or folder exists
export exists(path: String) => exists(path.text)

# Summary: Returns whether the specified file or folder exists
export exists(path: link) {
	=> internal.GetFileAttributesA(path) != 4294967295 # 0xFFFFFFFF
}

# Summary: Returns whether the path represents a folder in the filesystem
export is_folder(path: String) {
	=> is_folder(path.text)
}

# Summary: Returns whether the path represents a folder in the filesystem
export is_folder(path: link) {
	attributes = internal.GetFileAttributesA(path)
	if attributes == 4294967295 => false

	=> (attributes & internal.FILE_ATTRIBUTE_FOLDER) != 0 
}

# Summary: Returns the size of the specified file or folder
export size(path: String) {
	=> size(path.text)
}

# Summary: Returns the size of the specified file or folder
export size(path: link) {
	# TODO: Some folders do not work, that is, this function returns -1. This might be due to too long paths.
	if is_folder(path) {
		files = get_folder_files(path, true)
		total = 0

		loop (i = 0, i < files.size(), i++) {
			# Try to get the size of the file
			result = size(files[i].fullname)

			# If the size if negative, it means an error has occured
			if result < 0 => -1

			total += result
		}

		=> total
	}

	# Try to open the specified file
	file = internal.CreateFileA(path, internal.GENERIC_READ, internal.FILE_SHARE_READ, none, internal.OPEN_ALWAYS, internal.FILE_ATTRIBUTE_NORMAL, none)
	
	if file == none => -1

	# Try to get the size of the opened file
	size: large[1]
	
	if internal.GetFileSizeEx(file, size as link<large>) == 0 {
		internal.CloseHandle(file)
		=> -1
	}

	# Finally, release the handle
	internal.CloseHandle(file)
	=> size[0]
}

# Processes:
export start_process(executable: String, command_line_arguments: List<String>, working_folder: link) {
	startup_information = inline internal.StartupInformation()
	startup_information.size = 96
	zero(startup_information as link, 96)

	process_information = inline internal.ProcessInformation()

	# Combine all the command line arguments to a single command line string
	command_line = String.join(` `, command_line_arguments)

	# Try to create the requested process: Return -1, if the process creation fails, otherwise return the PID
	if not internal.CreateProcessA(executable.text, command_line.text, none as link, none as link, true, 0, none as link, working_folder, startup_information, process_information) => -1
	
	=> process_information.pid
}

export start_process(executable: String, command_line_arguments: List<String>, working_folder: String) {
	if working_folder as link != none => start_process(executable, command_line_arguments, working_folder.text)
	=> start_process(executable, command_line_arguments, none as link)
}

export start_process(executable: String, command_line_arguments: List<String>) {
	=> start_process(executable, command_line_arguments, none as link)
}

shell(command: String, working_folder: link) {
	shell = get_environment_variable('COMSPEC')
	if shell as link == none => -1

	startup_information = inline internal.StartupInformation()
	startup_information.size = 96
	zero(startup_information as link, 96)

	process_information = inline internal.ProcessInformation()

	command = String('/C ') + command

	# Try to create the requested process: Return -1, if the process creation fails, otherwise return the PID
	if not internal.CreateProcessA(shell.text, command.text, none as link, none as link, true, 0, none as link, working_folder, startup_information, process_information) => -1
	
	=> process_information.pid
}

export shell(command: String, working_folder: String) {
	if working_folder != none => shell(command, working_folder.text)
	=> shell(command, none as link)
}

export shell(command: String) {
	=> shell(command, none as link)
}

# Summary: Waits for the specified process to exit
export wait_for_exit(pid: large) {
	handle = internal.OpenProcess(internal.PROCESS_ACCESS_SYNCHRONIZE, false, pid)
	internal.WaitForSingleObject(handle, internal.INFINITE)
	internal.CloseHandle(handle)
}

# Command line:
export get_environment_variable(name: link) {
	# Try to get the size of the environment variable
	temporary: byte[1]
	size = internal.GetEnvironmentVariableA(name, temporary as link, 0)
	if size == 0 => none as String

	buffer = allocate(size)
	buffer[size] = 0
	internal.GetEnvironmentVariableA(name, buffer, size)

	=> String.from(buffer, size - 1)
}

# Summary: Returns the filename of the currently running process executable
export get_process_filename() {
	size = internal.MINIMUM_PROCESS_FILENAME_LENGTH
	filename = allocate(size)

	loop {
		length = internal.GetModuleFileNameA(none, filename, size)
		if length == 0 => none as String

		# The length of the filename must be at least one character shorter than the size of the buffer, so that the string terminator can be stored as well
		if length < size {
			filename[length] = 0

			result = String.from(filename, length).replace(`\\`, `/`)
			deallocate(filename)

			=> result
		}

		# Deallocate the filename buffer, because it is not large enough
		deallocate(filename)
		
		# Double the buffer size and allocate a new buffer
		size *= 2
		filename = allocate(size)
	}
}

# Summary: Returns the working directory of the currently running process
export get_process_working_folder() {
	# First, we need to get the size of the working folder. This can be done by requesting the working folder with an empty buffer
	size = internal.GetCurrentDirectoryA(0, none as link)

	buffer = allocate(size)
	buffer[size] = 0
	internal.GetCurrentDirectoryA(size, buffer)

	=> String.from(buffer, size - 1)
}

# Summary: Returns the folder which contains the current process executable
export get_process_folder() {
	filename = get_process_filename()
	if filename as link == none => none as String

	# Find the index of the last separator
	i = filename.last_index_of(`/`)
	if i == -1 => none as String
	
	# Include the separator to the folder path
	=> filename.slice(0, i + 1)
}

# Summary: Finds the specified ending character while taking into account special characters
find_argument_ending(text: String, ending: char, i: large) {
	loop (i < text.length, i++) {
		a = text[i]

		# Skip special characters since the ending can not be one
		if a == `\\` {
			i++
			continue
		}

		if a == ending => i
	}

	=> i
}

# Summary: Returns the list of the arguments passed to this application
export get_command_line_arguments() {
	text = String(internal.GetCommandLineA())
	empty = String('')

	arguments = List<String>()
	argument = empty

	i = 0
	p = 0

	loop (i < text.length, i++) {
		a = text[i]

		if a == `\"` {
			argument = argument + text.slice(p, i)
			j = find_argument_ending(text, `\"`, ++i)
			argument = argument + text.slice(i, j)
			i = j
			p = i + 1
		}
		else a == ` ` and (argument.length > 0 or i - p > 0) {
			arguments.add(argument + text.slice(p, i))
			argument = empty
			p = i + 1
		}
	}

	if argument.length == 0 and i - p == 0 => arguments

	arguments.add(argument + text.slice(p, i))
	=> arguments
}