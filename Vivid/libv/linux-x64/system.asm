.export system_read
system_read:
mov rax, 0 # System call: sys_read
syscall
ret

.export system_write
system_write:
mov rax, 1 # System call: sys_write
syscall
ret

.export system_open
system_open:
mov rax, 2 # System call: sys_open
syscall
ret

.export system_close
system_close:
mov rax, 3 # System call: sys_close
syscall
ret

.export system_status
system_status:
mov rax, 4 # System call: sys_stat
syscall
ret

.export system_memory_map
system_memory_map:
mov rax, 9 # System call: sys_mmap
mov r10, rcx
syscall
ret

.export system_memory_unmap
system_memory_unmap:
mov rax, 11 # System call: sys_munmap
syscall
ret

.export system_fork
system_fork:
mov rax, 57 # System call: sys_fork
syscall
ret

.export system_execute
system_execute:
mov rax, 59 # System call: sys_execve
syscall
ret

.export system_exit
system_exit:
mov rax, 60 # System call: sys_exit
syscall
ret

.export system_get_working_folder
system_get_working_folder:
mov rax, 79 # System call: sys_getcwd
syscall
ret

.export system_change_folder
system_change_folder:
mov rax, 80 # System call: sys_chdir
syscall
ret

.export system_remove_folder
system_remove_folder:
mov rax, 84 # System call: sys_rmdir
syscall
ret

.export system_unlink
system_unlink:
mov rax, 87 # System call: sys_unlink
syscall
ret

.export system_read_link
system_read_link:
mov rax, 89 # System call: sys_readlink
syscall
ret

.export system_get_directory_entries
system_get_directory_entries:
mov rax, 217 # System call: sys_getdents
syscall
ret

.export system_wait_id
system_wait_id:
mov rax, 247 # System call: sys_waitid
mov r10, rcx
syscall
ret

.export system_clock_get_time
system_clock_get_time:
mov rax, 228 # System call: sys_clock_gettime
syscall
ret