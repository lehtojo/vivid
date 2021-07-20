import get_stack_pointer(): link

export internal_init(root: link) {
	count = root.(link<large>)[0]

	arguments = List<String>(count, false)
	environment_variables = List<String>()

	# Load all the command line arguments
	loop (i = 0, i < count, i++) {
		argument = root.(link<link>)[i + 1]
		arguments.add(String.from(argument, length_of(argument)))
	}

	# Load all the environment variables
	i = count + 2 # Skip over the argument count, all the arguments and a none pointer

	# Load environment variables as long as the pointer is not none
	loop {
		environment_variable = root.(link<link>)[i]
		if environment_variable == none stop

		environment_variables.add(String.from(environment_variable, length_of(environment_variable)))
		i++
	}

	io.internal.arguments = arguments
	io.internal.environment_variables = environment_variables
	io.internal.executable = arguments[0]

	# Call the actual init function here
	init()
}

namespace io

namespace internal {
	executable: String
	arguments: List<String>
	environment_variables: List<String>

	constant FLAG_DIRECTORY = 65536
	constant FLAG_CREATE = 64
	constant FLAG_WRITE = 1
	constant FLAG_READ_AND_WRITE = 2

	constant DEFAULT_FILE_MODE = 438 # rw-rw-rw-

	# Summary: Reads data from the specified file and returns the number of bytes read
	import 'C' system_read(file_descriptor: large, buffer: link, size: large): large

	# Summary: Writes the specified data to the specified file
	import 'C' system_write(file_descriptor: large, buffer: link, size: large): large

	# Summary: Opens the specified file using the specified flags and returns a file descriptor
	import 'C' system_open(path: link, flags: large, mode: large): large

	# Summary: Closes the specified file descriptor
	import 'C' system_close(file_descriptor: large): large

	constant FILE_TYPE_MASK = 61440

	plain FileStatus {
		device: large
		inode: large
		hard_links: large
		mode: normal
		uid: normal
		gid: normal
		padding: normal
		device_id: large
		size: large
		block_size: large
		blocks: large
		modification_seconds: large
		modification_nanoseconds: large
		access_seconds: large
		access_nanoseconds: large
		creation_seconds: large
		creation_nanoseconds: large
		reserved: large[3]
	}

	import 'C' system_status(path: link, result: link): large

	constant FILE_TYPE_DIRECTORY = 4
	constant FILE_TYPE_REGULAR_FILE = 8
	
	constant FILE_MODE_DIRECTORY = 16384

	plain DirectoryEntry {
		inode: large
		next: large
		size: u16
		type: u8
	}

	# Summary: Writes directory entries to the specified buffer.
	# On success, the number of bytes written is returned.
	# On failure, -1 is returned.
	# When the end of the directory is reached, 0 is returned.
	import 'C' system_get_directory_entries(file_descriptor: large, buffer: link, size: large): large
}

FolderItem {
	fullname: String
	is_folder: bool

	init(fullname: String, is_folder: bool) {
		this.fullname = fullname
		this.is_folder = is_folder
	}
}

export get_folder_items(folder: String, all: bool) {
	# Try to open the specified folder
	file_descriptor = internal.system_open(folder.text, internal.FLAG_DIRECTORY, 0)
	if file_descriptor == -1 => none as List<FolderItem>

	items = List<FolderItem>()
	buffer: char[1000]

	loop {
		# Get the next batch of directory entries
		result = internal.system_get_directory_entries(file_descriptor, buffer as link, 1000)
		if result == -1 {
			internal.system_close(file_descriptor)
			=> none as List<FolderItem>
		}

		# If the result is zero, it means the end has been reached
		if result == 0 stop

		position = 0

		loop (position < result) {
			entry = (buffer + position) as internal.DirectoryEntry
			name = String(entry as link + 19)
			fullname = folder + `/` + name

			if entry.type == internal.FILE_TYPE_DIRECTORY {
				items.add(FolderItem(fullname, true))

				# If the user requested all items, load all items from the current directory recursively
				if all and not (name == '.' or name == '..') items.add_range(get_folder_items(fullname, true))
			}
			else {
				items.add(FolderItem(fullname, false))
			}

			position += entry.size
		}
	}

	internal.system_close(file_descriptor)
	=> items
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
	file_descriptor = internal.system_open(filename, internal.FLAG_CREATE | internal.FLAG_WRITE, internal.DEFAULT_FILE_MODE)
	if file_descriptor == -1 => false

	result = internal.system_write(file_descriptor, bytes.data, bytes.count)
	if result == -1 => false

	internal.system_close(file_descriptor)
	=> true
}

# Summary: Opens the specified file and returns its contents
export read_file(filename: String) => read_file(filename.text)

# Summary: Opens the specified file and returns its contents
export read_file(filename: link) {
	file_descriptor = internal.system_open(filename, internal.FLAG_CREATE, 0)
	if file_descriptor == -1 => Optional<Array<byte>>()

	buffer = allocate(100)
	size = 100
	position = 0

	loop {
		available = size - position
		result = internal.system_read(file_descriptor, buffer + position, available)

		# If the resutl is -1, it means the read failed
		if result == -1 {
			deallocate(buffer)
			internal.system_close(file_descriptor)
			=> Optional<Array<byte>>()
		}

		# If the result is zero, it means the end has been reached
		if result == 0 stop

		# If the buffer is too small, double its size
		if result == available {
			buffer = resize(buffer, size, size * 2)
			size *= 2
		}

		position += result
	}

	internal.system_close(file_descriptor)
	=> Optional<Array<byte>>(Array<byte>(buffer, position))
}

# Summary: Returns whether the specified file or folder exists
export exists(path: String) => exists(path.text)

# Summary: Returns whether the specified file or folder exists
export exists(path: link) {
	result = inline internal.FileStatus()
	=> internal.system_status(path, result as link) == 0
}

# Summary: Returns whether the path represents a folder in the filesystem
export is_folder(path: String) {
	=> is_folder(path.text)
}

# Summary: Returns whether the path represents a folder in the filesystem
export is_folder(path: link) {
	result = inline internal.FileStatus()
	=> internal.system_status(path, result as link) == 0 and (result.mode & internal.FILE_TYPE_MASK) == internal.FILE_MODE_DIRECTORY
}

# Summary: Returns the size of the specified file or folder
export size(path: String) {
	=> size(path.text)
}

# Summary: Returns the size of the specified file or folder
export size(path: link) {
	if is_folder(path) {
		files = get_folder_files(path, true)
		if files == none => 0

		total = 0
		loop file in files { total += size(file.fullname) }
		
		=> total
	}

	result = inline internal.FileStatus()
	if internal.system_status(path, result as link) != 0 => -1
	=> result.size
}

# TODO: Move into a separate namespace such as 'environment'
# Summary: Tries to find the specified enviroment variable and return its value, on failure none is returned.
export get_environment_variable(name: link) {
	start = String.from(name, length_of(name)) + '='

	loop environment_variable in internal.environment_variables {
		if environment_variable.starts_with(start) => environment_variable.slice(start.length, environment_variable.length)
	}

	=> none as String
}

# Summary: Returns the filename of the currently running process executable
export get_process_filename() {
	=> internal.executable
}

# Summary: Returns the folder which contains the current process executable
export get_process_folder() {
	filename = get_process_filename()
	if filename == none => none as String

	# Find the index of the last separator
	i = filename.last_index_of(`/`)
	if i == -1 => none as String
	
	# Include the separator to the folder path
	=> filename.slice(0, i + 1)
}

# Summary: Returns the list of the arguments passed to this application
export get_command_line_arguments() {
	=> internal.arguments
}