.section .text
.intel_syntax noprefix
.global main
main:
jmp _V4initv_rx

.extern _V14large_functionv
.extern _V17internal_allocatex_rPh

.global _V27scopes_nested_if_statementsxxxxxxxx_rx
_V27scopes_nested_if_statementsxxxxxxxx_rx:
push rbx
push rsi
push rdi
push rbp
push r12
push r13
push r14
push r15
sub rsp, 40
mov rbx, rcx
sal rbx, 1
lea rsi, [rdx*2+rdx]
lea rdi, [r8*4+r8]
mov rbp, rcx
mov r12, r8
mov r13, rdx
mov r14, r9
mov r15, [rsp+144]
test rbp, rbp
jle _V27scopes_nested_if_statementsxxxxxxxx_rx_L1
test r12, r12
jle _V27scopes_nested_if_statementsxxxxxxxx_rx_L3
call _V14large_functionv
_V27scopes_nested_if_statementsxxxxxxxx_rx_L3:
call _V14large_functionv
jmp _V27scopes_nested_if_statementsxxxxxxxx_rx_L0
_V27scopes_nested_if_statementsxxxxxxxx_rx_L1:
test r13, r13
jle _V27scopes_nested_if_statementsxxxxxxxx_rx_L5
test r14, r14
jle _V27scopes_nested_if_statementsxxxxxxxx_rx_L7
call _V14large_functionv
_V27scopes_nested_if_statementsxxxxxxxx_rx_L7:
call _V14large_functionv
jmp _V27scopes_nested_if_statementsxxxxxxxx_rx_L0
_V27scopes_nested_if_statementsxxxxxxxx_rx_L5:
test r15, r15
jle _V27scopes_nested_if_statementsxxxxxxxx_rx_L9
call _V14large_functionv
_V27scopes_nested_if_statementsxxxxxxxx_rx_L9:
call _V14large_functionv
_V27scopes_nested_if_statementsxxxxxxxx_rx_L0:
add rbp, r13
add rbp, r12
add rbp, r14
lea rax, [rbp+r15]
add rax, [rsp+152]
add rax, [rsp+160]
add rax, [rsp+168]
imul rax, rbx
imul rax, rsi
imul rax, rdi
add rsp, 40
pop r15
pop r14
pop r13
pop r12
pop rbp
pop rdi
pop rsi
pop rbx
ret

.global _V18scopes_single_loopxxxxxxxx_rx
_V18scopes_single_loopxxxxxxxx_rx:
push rbx
push rsi
push rdi
push rbp
push r12
push r13
push r14
push r15
sub rsp, 40
mov rbx, rcx
sal rbx, 1
lea rsi, [rdx*2+rdx]
lea rdi, [r8*4+r8]
xor rbp, rbp
mov r12, [rsp+168]
cmp rbp, r12
jge _V18scopes_single_loopxxxxxxxx_rx_L1
_V18scopes_single_loopxxxxxxxx_rx_L0:
mov r13, rcx
mov r14, rdx
mov r15, r8
mov qword ptr [rsp+136], r9
call _V14large_functionv
add rbp, 1
mov rcx, r13
mov rdx, r14
mov r8, r15
mov r9, [rsp+136]
cmp rbp, r12
jl _V18scopes_single_loopxxxxxxxx_rx_L0
_V18scopes_single_loopxxxxxxxx_rx_L1:
add rcx, rdx
add rcx, r8
add rcx, r9
add rcx, [rsp+144]
add rcx, [rsp+152]
add rcx, [rsp+160]
lea rax, [rcx+r12]
imul rax, rbx
imul rax, rsi
imul rax, rdi
add rsp, 40
pop r15
pop r14
pop r13
pop r12
pop rbp
pop rdi
pop rsi
pop rbx
ret

.global _V19scopes_nested_loopsxxxxxxxx_rx
_V19scopes_nested_loopsxxxxxxxx_rx:
push rbx
push rsi
push rdi
push rbp
push r12
push r13
push r14
push r15
sub rsp, 40
mov rbx, rcx
sal rbx, 1
lea rsi, [rdx*2+rdx]
lea rdi, [r8*4+r8]
xor rbp, rbp
mov r12, [rsp+168]
mov r13, [rsp+160]
cmp rbp, r12
jge _V19scopes_nested_loopsxxxxxxxx_rx_L1
_V19scopes_nested_loopsxxxxxxxx_rx_L0:
xor r14, r14
cmp r14, r13
jge _V19scopes_nested_loopsxxxxxxxx_rx_L4
_V19scopes_nested_loopsxxxxxxxx_rx_L3:
mov r15, rcx
mov qword ptr [rsp+120], rdx
mov qword ptr [rsp+128], r8
mov qword ptr [rsp+136], r9
call _V14large_functionv
add r14, 1
mov rcx, r15
mov rdx, [rsp+120]
mov r8, [rsp+128]
mov r9, [rsp+136]
cmp r14, r13
jl _V19scopes_nested_loopsxxxxxxxx_rx_L3
_V19scopes_nested_loopsxxxxxxxx_rx_L4:
mov r14, rcx
mov r15, rdx
mov qword ptr [rsp+128], r8
mov qword ptr [rsp+136], r9
call _V14large_functionv
add rbp, 1
mov rcx, r14
mov rdx, r15
mov r8, [rsp+128]
mov r9, [rsp+136]
cmp rbp, r12
jl _V19scopes_nested_loopsxxxxxxxx_rx_L0
_V19scopes_nested_loopsxxxxxxxx_rx_L1:
add rcx, rdx
add rcx, r8
add rcx, r9
add rcx, [rsp+144]
add rcx, [rsp+152]
add rcx, r13
lea rax, [rcx+r12]
imul rax, rbx
imul rax, rsi
imul rax, rdi
add rsp, 40
pop r15
pop r14
pop r13
pop r12
pop rbp
pop rdi
pop rsi
pop rbx
ret

.global _V4initv_rx
_V4initv_rx:
sub rsp, 56
mov rax, 1
add rsp, 56
ret
xor rcx, rcx
xor rdx, rdx
xor r8, r8
xor r9, r9
mov qword ptr [rsp+32], 0
mov qword ptr [rsp+40], 0
mov qword ptr [rsp+48], 0
mov qword ptr [rsp+56], 0
call _V27scopes_nested_if_statementsxxxxxxxx_rx
xor rcx, rcx
xor rdx, rdx
xor r8, r8
xor r9, r9
mov qword ptr [rsp+32], 0
mov qword ptr [rsp+40], 0
mov qword ptr [rsp+48], 0
mov qword ptr [rsp+56], 0
call _V18scopes_single_loopxxxxxxxx_rx
xor rcx, rcx
xor rdx, rdx
xor r8, r8
xor r9, r9
mov qword ptr [rsp+32], 0
mov qword ptr [rsp+40], 0
mov qword ptr [rsp+48], 0
mov qword ptr [rsp+56], 0
call _V19scopes_nested_loopsxxxxxxxx_rx
ret

.section .data

