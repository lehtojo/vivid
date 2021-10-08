.export _V4timev_rx
sub rsp, 16
mov rdi, rsp
mov rax, 96 # System call: sys_gettimeofday
syscall
imul rax, qword ptr [rsp], 10000000
mov rcx, 0x019DB1DED53E8000 # 116444736000000000 = 0x019DB1DED53E8000 = January 1, 1970 (Unix epoch) in 'ticks'
add rax, rcx
mov rcx, qword ptr [rsp+8] # Load the microseconds and convert them to multiple of 100 nanoseconds
imul rcx, 10
add rax, rcx # Add the microseconds
add rsp, 16
ret
