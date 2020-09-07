section .text
global _start
_start:
call _V4initv
mov rax, 60
xor rdi, rdi
syscall

extern _V8allocatex_rPh
extern _V14large_functionv

global _V27scopes_nested_if_statementsxxxxxxxx_rx
_V27scopes_nested_if_statementsxxxxxxxx_rx:
push rbx
push rbp
push r12
push r13
push r14
push r15
sub rsp, 40
mov rbx, rdi
sal rbx, 1
lea rbp, [rsi*2+rsi]
lea r12, [rdx*4+rdx]
test rdi, rdi
jle _V27scopes_nested_if_statementsxxxxxxxx_rx_L1
test rdx, rdx
jle _V27scopes_nested_if_statementsxxxxxxxx_rx_L3
mov r13, rcx
mov r14, rdx
mov r15, rsi
mov qword [rsp+32], rdi
mov qword [rsp+24], r8
mov qword [rsp+16], r9
call _V14large_functionv
mov rdi, [rsp+32]
mov rsi, r15
mov rdx, r14
mov rcx, r13
mov r8, [rsp+24]
mov r9, [rsp+16]
_V27scopes_nested_if_statementsxxxxxxxx_rx_L3:
mov r13, rcx
mov r14, rdx
mov r15, rsi
mov qword [rsp+32], rdi
mov qword [rsp+24], r8
mov qword [rsp+16], r9
call _V14large_functionv
mov rdi, [rsp+32]
mov rsi, r15
mov rdx, r14
mov rcx, r13
mov r8, [rsp+24]
mov r9, [rsp+16]
jmp _V27scopes_nested_if_statementsxxxxxxxx_rx_L0
_V27scopes_nested_if_statementsxxxxxxxx_rx_L1:
test rsi, rsi
jle _V27scopes_nested_if_statementsxxxxxxxx_rx_L5
test rcx, rcx
jle _V27scopes_nested_if_statementsxxxxxxxx_rx_L7
mov r13, rcx
mov r14, rdx
mov r15, rsi
mov qword [rsp+32], rdi
mov qword [rsp+24], r8
mov qword [rsp+16], r9
call _V14large_functionv
mov rdi, [rsp+32]
mov rsi, r15
mov rdx, r14
mov rcx, r13
mov r8, [rsp+24]
mov r9, [rsp+16]
_V27scopes_nested_if_statementsxxxxxxxx_rx_L7:
mov r13, rcx
mov r14, rdx
mov r15, rsi
mov qword [rsp+32], rdi
mov qword [rsp+24], r8
mov qword [rsp+16], r9
call _V14large_functionv
mov rdi, [rsp+32]
mov rsi, r15
mov rdx, r14
mov rcx, r13
mov r8, [rsp+24]
mov r9, [rsp+16]
jmp _V27scopes_nested_if_statementsxxxxxxxx_rx_L0
_V27scopes_nested_if_statementsxxxxxxxx_rx_L5:
test r8, r8
jle _V27scopes_nested_if_statementsxxxxxxxx_rx_L9
mov r13, rcx
mov r14, rdx
mov r15, rsi
mov qword [rsp+32], rdi
mov qword [rsp+24], r8
mov qword [rsp+16], r9
call _V14large_functionv
mov rdi, [rsp+32]
mov rsi, r15
mov rdx, r14
mov rcx, r13
mov r8, [rsp+24]
mov r9, [rsp+16]
_V27scopes_nested_if_statementsxxxxxxxx_rx_L9:
mov r13, rcx
mov r14, rdx
mov r15, rsi
mov qword [rsp+32], rdi
mov qword [rsp+24], r8
mov qword [rsp+16], r9
call _V14large_functionv
mov rdi, [rsp+32]
mov rsi, r15
mov rdx, r14
mov rcx, r13
mov r8, [rsp+24]
mov r9, [rsp+16]
_V27scopes_nested_if_statementsxxxxxxxx_rx_L0:
add rdi, rsi
add rdi, rdx
add rdi, rcx
add rdi, r8
add rdi, r9
add rdi, [rsp+96]
add rdi, [rsp+104]
imul rdi, rbx
imul rdi, rbp
imul rdi, r12
mov rax, rdi
add rsp, 40
pop r15
pop r14
pop r13
pop r12
pop rbp
pop rbx
ret

global _V18scopes_single_loopxxxxxxxx_rx
_V18scopes_single_loopxxxxxxxx_rx:
push rbx
push rbp
push r12
push r13
push r14
push r15
sub rsp, 56
mov rbx, rdi
sal rbx, 1
lea rbp, [rsi*2+rsi]
lea r12, [rdx*4+rdx]
mov r13, [rsp+120]
xor r10, r10
cmp r10, r13
jge _V18scopes_single_loopxxxxxxxx_rx_L1
_V18scopes_single_loopxxxxxxxx_rx_L0:
mov r14, rcx
mov r15, rdx
mov qword [rsp+48], rsi
mov qword [rsp+40], rdi
mov qword [rsp+32], r8
mov qword [rsp+24], r9
mov qword [rsp+16], r10
call _V14large_functionv
add qword [rsp+16], 1
mov rcx, [rsp+16]
cmp rcx, r13
mov r10, rcx
mov rdi, [rsp+40]
mov rsi, [rsp+48]
mov rdx, r15
mov rcx, r14
mov r8, [rsp+32]
mov r9, [rsp+24]
jl _V18scopes_single_loopxxxxxxxx_rx_L0
_V18scopes_single_loopxxxxxxxx_rx_L1:
add rdi, rsi
add rdi, rdx
add rdi, rcx
add rdi, r8
add rdi, r9
add rdi, [rsp+112]
add rdi, r13
imul rdi, rbx
imul rdi, rbp
imul rdi, r12
mov rax, rdi
add rsp, 56
pop r15
pop r14
pop r13
pop r12
pop rbp
pop rbx
ret

global _V19scopes_nested_loopsxxxxxxxx_rx
_V19scopes_nested_loopsxxxxxxxx_rx:
push rbx
push rbp
push r12
push r13
push r14
push r15
sub rsp, 72
mov rbx, rdi
sal rbx, 1
lea rbp, [rsi*2+rsi]
lea r12, [rdx*4+rdx]
mov r13, [rsp+136]
mov r14, [rsp+128]
xor r10, r10
cmp r10, r13
jge _V19scopes_nested_loopsxxxxxxxx_rx_L1
_V19scopes_nested_loopsxxxxxxxx_rx_L0:
xor r11, r11
cmp r11, r14
jge _V19scopes_nested_loopsxxxxxxxx_rx_L3
_V19scopes_nested_loopsxxxxxxxx_rx_L2:
mov r15, rcx
mov qword [rsp+64], rdx
mov qword [rsp+56], rsi
mov qword [rsp+48], rdi
mov qword [rsp+40], r8
mov qword [rsp+32], r9
mov qword [rsp+24], r10
mov qword [rsp+16], r11
call _V14large_functionv
add qword [rsp+16], 1
mov rcx, [rsp+16]
cmp rcx, r14
mov r11, rcx
mov r10, [rsp+24]
mov rdi, [rsp+48]
mov rsi, [rsp+56]
mov rdx, [rsp+64]
mov rcx, r15
mov r8, [rsp+40]
mov r9, [rsp+32]
jl _V19scopes_nested_loopsxxxxxxxx_rx_L2
_V19scopes_nested_loopsxxxxxxxx_rx_L3:
mov r15, rcx
mov qword [rsp+64], rdx
mov qword [rsp+56], rsi
mov qword [rsp+48], rdi
mov qword [rsp+40], r8
mov qword [rsp+32], r9
mov qword [rsp+24], r10
call _V14large_functionv
add qword [rsp+24], 1
mov rcx, [rsp+24]
cmp rcx, r13
mov r10, rcx
mov rdi, [rsp+48]
mov rsi, [rsp+56]
mov rdx, [rsp+64]
mov rcx, r15
mov r8, [rsp+40]
mov r9, [rsp+32]
jl _V19scopes_nested_loopsxxxxxxxx_rx_L0
_V19scopes_nested_loopsxxxxxxxx_rx_L1:
add rdi, rsi
add rdi, rdx
add rdi, rcx
add rdi, r8
add rdi, r9
add rdi, r14
add rdi, r13
imul rdi, rbx
imul rdi, rbp
imul rdi, r12
mov rax, rdi
add rsp, 72
pop r15
pop r14
pop r13
pop r12
pop rbp
pop rbx
ret

_V4initv:
sub rsp, 24
xor rdi, rdi
xor rsi, rsi
xor rdx, rdx
xor rcx, rcx
xor r8, r8
xor r9, r9
mov qword [rsp], 0
mov qword [rsp+8], 0
call _V27scopes_nested_if_statementsxxxxxxxx_rx
xor rdi, rdi
xor rsi, rsi
xor rdx, rdx
xor rcx, rcx
xor r8, r8
xor r9, r9
mov qword [rsp], 0
mov qword [rsp+8], 0
call _V18scopes_single_loopxxxxxxxx_rx
xor rdi, rdi
xor rsi, rsi
xor rdx, rdx
xor rcx, rcx
xor r8, r8
xor r9, r9
mov qword [rsp], 0
mov qword [rsp+8], 0
call _V19scopes_nested_loopsxxxxxxxx_rx
add rsp, 24
ret