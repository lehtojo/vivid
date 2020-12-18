.section .text
.intel_syntax noprefix
.global main
main:
jmp _V4initv_rx

.extern _V14large_functionv
.extern _V17internal_allocatex_rPh

.global _V5loopsxx_rx
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

.global _V12forever_loopv_rx
_V12forever_loopv_rx:
xor rax, rax
_V12forever_loopv_rx_L1:
_V12forever_loopv_rx_L0:
add rax, 1
jmp _V12forever_loopv_rx_L0
_V12forever_loopv_rx_L2:
ret

.global _V16conditional_loopx_rx
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

.global _V23conditional_action_loopx_rx
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

.global _V15normal_for_loopxx_rx
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

.global _V25normal_for_loop_with_stopxx_rx
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

.global _V29normal_for_loop_with_continuexx_rx
_V29normal_for_loop_with_continuexx_rx:
mov rax, rcx
xor r8, r8
cmp r8, rdx
jge _V29normal_for_loop_with_continuexx_rx_L1
_V29normal_for_loop_with_continuexx_rx_L0:
mov r9, rax
mov rax, r8
mov r10, rdx
cqo
mov r11, 2
idiv r11
test rdx, rdx
mov rdx, r10
mov rax, r9
jne _V29normal_for_loop_with_continuexx_rx_L3
add rax, 1
jmp _V29normal_for_loop_with_continuexx_rx_L0
_V29normal_for_loop_with_continuexx_rx_L3:
add rax, r8
add r8, 1
cmp r8, rdx
jl _V29normal_for_loop_with_continuexx_rx_L0
_V29normal_for_loop_with_continuexx_rx_L1:
ret

.global _V16nested_for_loopsPhx_rx
_V16nested_for_loopsPhx_rx:
push rbx
push rsi
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
cqo
mov rsi, 2
idiv rsi
test rdx, rdx
mov rax, r11
mov rdx, rbx
jne _V16nested_for_loopsPhx_rx_L12
mov r11, rax
mov rax, r9
mov rbx, rdx
cqo
mov rsi, 2
idiv rsi
test rdx, rdx
mov rax, r11
mov rdx, rbx
jne _V16nested_for_loopsPhx_rx_L12
mov r11, rax
mov rbx, rdx
cqo
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
mov byte ptr [rcx+r11], 100
jmp _V16nested_for_loopsPhx_rx_L11
_V16nested_for_loopsPhx_rx_L12:
mov r11, rax
imul r11, rdx
imul r11, rdx
mov rbx, r9
imul rbx, rdx
add r11, rbx
add r11, r10
mov byte ptr [rcx+r11], 0
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
pop rsi
pop rbx
ret

.global _V38normal_for_loop_with_memory_evacuationxx
_V38normal_for_loop_with_memory_evacuationxx:
push rbx
push rsi
push rdi
sub rsp, 32
mov rbx, rcx
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
add rsp, 32
pop rdi
pop rsi
pop rbx
ret

.global _V4initv_rx
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

.section .data

