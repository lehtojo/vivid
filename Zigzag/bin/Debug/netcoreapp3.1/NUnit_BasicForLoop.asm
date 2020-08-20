section .text
global _start
_start:
call _V4initv_rx
mov rax, 60
xor rdi, rdi
syscall

extern _V8allocatex_rPh
extern _V14large_functionv

global _V14basic_for_loopxx_rx
_V14basic_for_loopxx_rx:
xor rax, rax
xor rcx, rcx
mov rdx, rdi
cmp rax, rsi
jge _V14basic_for_loopxx_rx_L1
_V14basic_for_loopxx_rx_L0:
add rdx, rcx
add rcx, 3
add rax, 1
cmp rax, rsi
jl _V14basic_for_loopxx_rx_L0
_V14basic_for_loopxx_rx_L1:
mov rax, rdx
ret

global _V12forever_loopv_rx
_V12forever_loopv_rx:
xor rax, rax
_V12forever_loopv_rx_L0:
add rax, 1
jmp _V12forever_loopv_rx_L0
ret

global _V16conditional_loopx_rx
_V16conditional_loopx_rx:
cmp rdi, 10
jge _V16conditional_loopx_rx_L1
_V16conditional_loopx_rx_L0:
add rdi, 1
cmp rdi, 10
jl _V16conditional_loopx_rx_L0
_V16conditional_loopx_rx_L1:
mov rax, rdi
ret

global _V23conditional_action_loopx_rx
_V23conditional_action_loopx_rx:
cmp rdi, 1000
jge _V23conditional_action_loopx_rx_L1
_V23conditional_action_loopx_rx_L0:
sal rdi, 1
cmp rdi, 1000
jl _V23conditional_action_loopx_rx_L0
_V23conditional_action_loopx_rx_L1:
mov rax, rdi
ret

global _V15normal_for_loopxx_rx
_V15normal_for_loopxx_rx:
xor rcx, rcx
mov rax, rdi
cmp rcx, rsi
jge _V15normal_for_loopxx_rx_L1
_V15normal_for_loopxx_rx_L0:
add rax, rcx
add rcx, 1
cmp rcx, rsi
jl _V15normal_for_loopxx_rx_L0
_V15normal_for_loopxx_rx_L1:
ret

global _V25normal_for_loop_with_stopxx_rx
_V25normal_for_loop_with_stopxx_rx:
xor rcx, rcx
mov rax, rdi
cmp rcx, rsi
jg _V25normal_for_loop_with_stopxx_rx_L1
_V25normal_for_loop_with_stopxx_rx_L0:
cmp rcx, 100
jle _V25normal_for_loop_with_stopxx_rx_L2
mov rax, -1
jmp _V25normal_for_loop_with_stopxx_rx_L1
mov rax, -1
_V25normal_for_loop_with_stopxx_rx_L2:
add rax, rcx
add rcx, 1
cmp rcx, rsi
jle _V25normal_for_loop_with_stopxx_rx_L0
_V25normal_for_loop_with_stopxx_rx_L1:
ret

global _V16nested_for_loopsPhx_rx
_V16nested_for_loopsPhx_rx:
xor rax, rax
xor rcx, rcx
cmp rcx, rsi
jge _V16nested_for_loopsPhx_rx_L1
_V16nested_for_loopsPhx_rx_L0:
xor rdx, rdx
cmp rdx, rsi
jge _V16nested_for_loopsPhx_rx_L3
_V16nested_for_loopsPhx_rx_L2:
test rdx, rdx
jne _V16nested_for_loopsPhx_rx_L4
add rax, 1
_V16nested_for_loopsPhx_rx_L4:
xor r8, r8
cmp r8, rsi
jge _V16nested_for_loopsPhx_rx_L7
_V16nested_for_loopsPhx_rx_L6:
mov r9, rax
mov rax, r8
mov r10, rdx
xor rdx, rdx
mov r11, 2
idiv r11
test rdx, rdx
mov rdx, r10
mov rax, r9
jne _V16nested_for_loopsPhx_rx_L9
mov r9, rax
mov rax, rdx
mov r10, rdx
xor rdx, rdx
mov r11, 2
idiv r11
test rdx, rdx
mov rdx, r10
mov rax, r9
jne _V16nested_for_loopsPhx_rx_L9
mov r9, rax
mov rax, rcx
mov r10, rdx
xor rdx, rdx
mov r11, 2
idiv r11
test rdx, rdx
mov rdx, r10
mov rax, r9
jne _V16nested_for_loopsPhx_rx_L9
mov r9, rcx
imul r9, rsi
imul r9, rsi
mov r10, rdx
imul r10, rsi
add r9, r10
add r9, r8
mov byte [rdi+r9], 100
jmp _V16nested_for_loopsPhx_rx_L8
_V16nested_for_loopsPhx_rx_L9:
mov r9, rcx
imul r9, rsi
imul r9, rsi
mov r10, rdx
imul r10, rsi
add r9, r10
add r9, r8
mov byte [rdi+r9], 0
_V16nested_for_loopsPhx_rx_L8:
test r8, r8
jne _V16nested_for_loopsPhx_rx_L13
add rax, 1
_V16nested_for_loopsPhx_rx_L13:
add r8, 1
cmp r8, rsi
jl _V16nested_for_loopsPhx_rx_L6
_V16nested_for_loopsPhx_rx_L7:
add rdx, 1
cmp rdx, rsi
jl _V16nested_for_loopsPhx_rx_L2
_V16nested_for_loopsPhx_rx_L3:
test rcx, rcx
jne _V16nested_for_loopsPhx_rx_L15
add rax, 1
_V16nested_for_loopsPhx_rx_L15:
add rcx, 1
cmp rcx, rsi
jl _V16nested_for_loopsPhx_rx_L0
_V16nested_for_loopsPhx_rx_L1:
ret

global _V38normal_for_loop_with_memory_evacuationxx
_V38normal_for_loop_with_memory_evacuationxx:
push rbx
push rbp
push r12
sub rsp, 16
mov rbx, rdi
mov rbp, rsi
mov rax, rbx
cmp rax, rbp
jge _V38normal_for_loop_with_memory_evacuationxx_L1
_V38normal_for_loop_with_memory_evacuationxx_L0:
mov r12, rax
call _V14large_functionv
add r12, 1
cmp r12, rbp
mov rax, r12
jl _V38normal_for_loop_with_memory_evacuationxx_L0
_V38normal_for_loop_with_memory_evacuationxx_L1:
add rsp, 16
pop r12
pop rbp
pop rbx
ret

_V4initv_rx:
sub rsp, 8
mov rax, 1
add rsp, 8
ret
xor rdi, rdi
xor rsi, rsi
call _V14basic_for_loopxx_rx
call _V12forever_loopv_rx
xor rdi, rdi
call _V16conditional_loopx_rx
xor rdi, rdi
call _V23conditional_action_loopx_rx
xor rdi, rdi
xor rsi, rsi
call _V15normal_for_loopxx_rx
xor rdi, rdi
xor rsi, rsi
call _V25normal_for_loop_with_stopxx_rx
xor rdi, rdi
xor rsi, rsi
call _V16nested_for_loopsPhx_rx
xor rdi, rdi
xor rsi, rsi
call _V38normal_for_loop_with_memory_evacuationxx
ret