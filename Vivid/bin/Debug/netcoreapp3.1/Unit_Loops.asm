section .text
global main
main:
jmp _V4initv_rx

extern _V8allocatex_rPh
extern _V14large_functionv

global _V5loopsxx_rx
export _V5loopsxx_rx
_V5loopsxx_rx:
mov rax, rcx
xor r8, r8
xor r9, r9
cmp r8, rdx
jge _V5loopsxx_rx_L1
_V5loopsxx_rx_L0:
add rax, r9
add r9, 3
add r8, 1
cmp r8, rdx
jl _V5loopsxx_rx_L0
_V5loopsxx_rx_L1:
ret

global _V12forever_loopv_rx
export _V12forever_loopv_rx
_V12forever_loopv_rx:
xor rax, rax
_V12forever_loopv_rx_L1:
_V12forever_loopv_rx_L0:
add rax, 1
jmp _V12forever_loopv_rx_L0
_V12forever_loopv_rx_L2:
ret

global _V16conditional_loopx_rx
export _V16conditional_loopx_rx
_V16conditional_loopx_rx:
cmp rcx, 10
jge _V16conditional_loopx_rx_L1
_V16conditional_loopx_rx_L0:
add rcx, 1
cmp rcx, 10
jl _V16conditional_loopx_rx_L0
_V16conditional_loopx_rx_L1:
mov rax, rcx
ret

global _V23conditional_action_loopx_rx
export _V23conditional_action_loopx_rx
_V23conditional_action_loopx_rx:
cmp rcx, 1000
jge _V23conditional_action_loopx_rx_L1
_V23conditional_action_loopx_rx_L0:
sal rcx, 1
cmp rcx, 1000
jl _V23conditional_action_loopx_rx_L0
_V23conditional_action_loopx_rx_L1:
mov rax, rcx
ret

global _V15normal_for_loopxx_rx
export _V15normal_for_loopxx_rx
_V15normal_for_loopxx_rx:
mov rax, rcx
xor r8, r8
cmp r8, rdx
jge _V15normal_for_loopxx_rx_L1
_V15normal_for_loopxx_rx_L0:
add rax, r8
add r8, 1
cmp r8, rdx
jl _V15normal_for_loopxx_rx_L0
_V15normal_for_loopxx_rx_L1:
ret

global _V25normal_for_loop_with_stopxx_rx
export _V25normal_for_loop_with_stopxx_rx
_V25normal_for_loop_with_stopxx_rx:
mov rax, rcx
xor r8, r8
cmp r8, rdx
jg _V25normal_for_loop_with_stopxx_rx_L1
_V25normal_for_loop_with_stopxx_rx_L0:
cmp r8, 100
jle _V25normal_for_loop_with_stopxx_rx_L3
mov rax, -1
jmp _V25normal_for_loop_with_stopxx_rx_L1
_V25normal_for_loop_with_stopxx_rx_L3:
add rax, r8
add r8, 1
cmp r8, rdx
jle _V25normal_for_loop_with_stopxx_rx_L0
_V25normal_for_loop_with_stopxx_rx_L1:
ret

global _V16nested_for_loopsPhx_rx
export _V16nested_for_loopsPhx_rx
_V16nested_for_loopsPhx_rx:
push rbx
push rsi
sub rsp, 16
xor rax, rax
xor r8, r8
cmp rax, rdx
jge _V16nested_for_loopsPhx_rx_L1
_V16nested_for_loopsPhx_rx_L0:
xor r9, r9
cmp r9, rdx
jge _V16nested_for_loopsPhx_rx_L4
_V16nested_for_loopsPhx_rx_L3:
test r9, r9
jne _V16nested_for_loopsPhx_rx_L6
add r8, 1
_V16nested_for_loopsPhx_rx_L6:
xor r10, r10
cmp r10, rdx
jge _V16nested_for_loopsPhx_rx_L9
_V16nested_for_loopsPhx_rx_L8:
mov r11, rax
mov rax, r10
mov rbx, rdx
xor rdx, rdx
mov rsi, 2
idiv rsi
test rdx, rdx
mov rax, r11
mov rdx, rbx
jne _V16nested_for_loopsPhx_rx_L12
mov r11, rax
mov rax, r9
mov rbx, rdx
xor rdx, rdx
mov rsi, 2
idiv rsi
test rdx, rdx
mov rax, r11
mov rdx, rbx
jne _V16nested_for_loopsPhx_rx_L12
mov r11, rax
mov rbx, rdx
xor rdx, rdx
mov rsi, 2
idiv rsi
test rdx, rdx
mov rax, r11
mov rdx, rbx
jne _V16nested_for_loopsPhx_rx_L12
mov r11, rax
imul r11, rdx
imul r11, rdx
mov rbx, r9
imul rbx, rdx
add r11, rbx
add r11, r10
mov byte [rcx+r11], 100
jmp _V16nested_for_loopsPhx_rx_L11
_V16nested_for_loopsPhx_rx_L12:
mov r11, rax
imul r11, rdx
imul r11, rdx
mov rbx, r9
imul rbx, rdx
add r11, rbx
add r11, r10
mov byte [rcx+r11], 0
_V16nested_for_loopsPhx_rx_L11:
test r10, r10
jne _V16nested_for_loopsPhx_rx_L16
add r8, 1
_V16nested_for_loopsPhx_rx_L16:
add r10, 1
cmp r10, rdx
jl _V16nested_for_loopsPhx_rx_L8
_V16nested_for_loopsPhx_rx_L9:
add r9, 1
cmp r9, rdx
jl _V16nested_for_loopsPhx_rx_L3
_V16nested_for_loopsPhx_rx_L4:
test rax, rax
jne _V16nested_for_loopsPhx_rx_L20
add r8, 1
_V16nested_for_loopsPhx_rx_L20:
add rax, 1
cmp rax, rdx
jl _V16nested_for_loopsPhx_rx_L0
_V16nested_for_loopsPhx_rx_L1:
mov rax, r8
add rsp, 16
pop rsi
pop rbx
ret

global _V38normal_for_loop_with_memory_evacuationxx
export _V38normal_for_loop_with_memory_evacuationxx
_V38normal_for_loop_with_memory_evacuationxx:
push rbx
push rsi
push rdi
sub rsp, 48
mov rax, rcx
mov rbx, rax
mov rsi, rcx
mov rdi, rdx
cmp rbx, rdi
jge _V38normal_for_loop_with_memory_evacuationxx_L1
_V38normal_for_loop_with_memory_evacuationxx_L0:
call _V14large_functionv
add rbx, 1
cmp rbx, rdi
jl _V38normal_for_loop_with_memory_evacuationxx_L0
_V38normal_for_loop_with_memory_evacuationxx_L1:
add rsp, 48
pop rdi
pop rsi
pop rbx
ret

_V4initv_rx:
sub rsp, 40
mov rax, 1
add rsp, 40
ret
xor rcx, rcx
xor rdx, rdx
call _V5loopsxx_rx
call _V12forever_loopv_rx
xor rcx, rcx
call _V16conditional_loopx_rx
xor rcx, rcx
call _V23conditional_action_loopx_rx
xor rcx, rcx
xor rdx, rdx
call _V15normal_for_loopxx_rx
xor rcx, rcx
xor rdx, rdx
call _V25normal_for_loop_with_stopxx_rx
xor rcx, rcx
xor rdx, rdx
call _V16nested_for_loopsPhx_rx
xor rcx, rcx
xor rdx, rdx
call _V38normal_for_loop_with_memory_evacuationxx
ret