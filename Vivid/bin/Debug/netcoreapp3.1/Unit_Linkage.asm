.section .text
.intel_syntax noprefix
.file 1 "Sandbox.v"
.global main
main:
jmp _V4initv_rx

.extern _V14large_functionv
.extern _V17internal_allocatex_rPh

.global _V9linkage_1x_rx
_V9linkage_1x_rx:
mov rax, rcx
add rax, 1
add rax, rcx
ret

.global _V9linkage_2x_rx
_V9linkage_2x_rx:
sal rcx, 1
lea rax, [rcx+2]
ret

.global _V9linkage_3x_rx
_V9linkage_3x_rx:
mov rax, rcx
xor rdx, rdx
cmp rdx, 3
jge _V9linkage_3x_rx_L1
_V9linkage_3x_rx_L0:
mov rax, rdx
lea rcx, [rdx+1]
add rdx, 1
cmp rdx, 3
jl _V9linkage_3x_rx_L0
_V9linkage_3x_rx_L1:
add rax, rcx
ret

.global _V9linkage_4x_rx
_V9linkage_4x_rx:
mov rax, rcx
mov rdx, rax
mov r8, rdx
mov r9, r8
xor r10, r10
cmp r10, 5
jge _V9linkage_4x_rx_L1
_V9linkage_4x_rx_L0:
add rax, 1
add rdx, 2
add r8, 4
add r9, 8
add r10, 1
cmp r10, 5
jl _V9linkage_4x_rx_L0
_V9linkage_4x_rx_L1:
add rax, rdx
add rax, r8
add rax, r9
ret

.global _V9linkage_5x_rx
_V9linkage_5x_rx:
push rbx
push rsi
push rdi
push rbp
push r12
push r13
push r14
push r15
sub rsp, 56
mov rbx, rcx
mov rsi, rbx
mov rdi, rsi
mov rbp, rdi
mov r12, rbp
mov r13, r12
mov r14, r13
mov r15, r14
mov rdx, r15
mov qword ptr [rsp+48], rdx
mov qword ptr [rsp+128], rcx
mov qword ptr [rsp+40], rdx
call _V14large_functionv
xor rax, rax
mov rcx, [rsp+40]
mov rdx, [rsp+48]
cmp rax, 5
jge _V9linkage_5x_rx_L1
_V9linkage_5x_rx_L0:
add rbx, 1
add rsi, 2
add rdi, 3
add rbp, 4
add r12, 5
add r13, 6
add r14, 7
add r15, 8
add rcx, 9
add rdx, 10
add rax, 1
cmp rax, 5
jl _V9linkage_5x_rx_L0
_V9linkage_5x_rx_L1:
add rbx, rsi
add rbx, rdi
add rbx, rbp
add rbx, r12
add rbx, r13
add rbx, r14
add rbx, r15
add rbx, rcx
lea rax, [rbx+rdx]
add rsp, 56
pop r15
pop r14
pop r13
pop r12
pop rbp
pop rdi
pop rsi
pop rbx
ret

.global _V16linked_variablesxx_rx
_V16linked_variablesxx_rx:
push rbx
push rsi
push rdi
push rbp
sub rsp, 40
mov rbx, rcx
xor rsi, rsi
mov rdi, rdx
cmp rsi, rdi
jge _V16linked_variablesxx_rx_L1
_V16linked_variablesxx_rx_L0:
mov rbp, rcx
call _V14large_functionv
add rsi, 1
mov rcx, rbp
cmp rsi, rdi
jl _V16linked_variablesxx_rx_L0
_V16linked_variablesxx_rx_L1:
lea rax, [rbx+rcx]
add rsp, 40
pop rbp
pop rdi
pop rsi
pop rbx
ret

.global _V18linked_variables_2xx_rx
_V18linked_variables_2xx_rx:
push rbx
push rsi
push rdi
push rbp
sub rsp, 40
xor rbx, rbx
mov rsi, rdx
xor rdi, rdi
mov rbp, rcx
cmp rbx, rsi
jge _V18linked_variables_2xx_rx_L1
_V18linked_variables_2xx_rx_L0:
call _V14large_functionv
add rdi, rbp
add rbx, 1
cmp rbx, rsi
jl _V18linked_variables_2xx_rx_L0
_V18linked_variables_2xx_rx_L1:
lea rax, [rdi+rbp]
add rsp, 40
pop rbp
pop rdi
pop rsi
pop rbx
ret

_V4initv_rx:
sub rsp, 40
mov rax, 1
add rsp, 40
ret
mov rcx, 10
call _V9linkage_1x_rx
mov rcx, 10
call _V9linkage_2x_rx
mov rcx, 10
call _V9linkage_3x_rx
mov rcx, 10
call _V9linkage_4x_rx
mov rcx, 10
call _V9linkage_5x_rx
xor rcx, rcx
xor rdx, rdx
call _V16linked_variablesxx_rx
xor rcx, rcx
xor rdx, rdx
call _V18linked_variables_2xx_rx
ret

_V8allocatex_rPh:
push rbx
push rsi
sub rsp, 40
mov r8, [rip+_VN10Allocation_current]
test r8, r8
je _V8allocatex_rPh_L0
mov rdx, [r8+16]
lea r9, [rdx+rcx]
cmp r9, 1000000
jg _V8allocatex_rPh_L0
lea r9, [rdx+rcx]
mov qword ptr [r8+16], r9
lea r9, [rdx+rcx]
mov rax, [r8+8]
add rax, rdx
add rsp, 40
pop rsi
pop rbx
ret
_V8allocatex_rPh_L0:
mov rbx, rcx
mov rcx, 1000000
call _V17internal_allocatex_rPh
mov rcx, 24
mov rsi, rax
call _V17internal_allocatex_rPh
mov qword ptr [rax+8], rsi
mov qword ptr [rax+16], rbx
mov qword ptr [rip+_VN10Allocation_current], rax
mov rax, rsi
add rsp, 40
pop rsi
pop rbx
ret

_V8inheritsPhS__rx:
push rbx
push rsi
mov r8, [rcx]
mov r9, [rdx]
movzx r10, byte ptr [r9]
xor rax, rax
_V8inheritsPhS__rx_L1:
_V8inheritsPhS__rx_L0:
movzx rcx, byte ptr [r8+rax]
add rax, 1
cmp rcx, r10
jnz _V8inheritsPhS__rx_L4
mov r11, rcx
mov rbx, 1
_V8inheritsPhS__rx_L7:
_V8inheritsPhS__rx_L6:
movzx r11, byte ptr [r8+rax]
movzx rsi, byte ptr [r9+rbx]
add rax, 1
add rbx, 1
cmp r11, rsi
jz _V8inheritsPhS__rx_L9
cmp r11, 1
jne _V8inheritsPhS__rx_L9
test rsi, rsi
jne _V8inheritsPhS__rx_L9
mov rax, 1
pop rsi
pop rbx
ret
_V8inheritsPhS__rx_L9:
jmp _V8inheritsPhS__rx_L6
_V8inheritsPhS__rx_L8:
jmp _V8inheritsPhS__rx_L3
_V8inheritsPhS__rx_L4:
cmp rcx, 2
jne _V8inheritsPhS__rx_L3
xor rax, rax
pop rsi
pop rbx
ret
_V8inheritsPhS__rx_L3:
jmp _V8inheritsPhS__rx_L0
_V8inheritsPhS__rx_L2:
pop rsi
pop rbx
ret

.section .data

_VN10Allocation_current:
.quad 0

_VN4Page_configuration:
.quad _VN4Page_descriptor

_VN4Page_descriptor:
.quad _VN4Page_descriptor_0
.long 24
.long 0

_VN4Page_descriptor_0:
.ascii "Page"
.byte 0
.byte 1
.byte 2
.byte 0

_VN10Allocation_configuration:
.quad _VN10Allocation_descriptor

_VN10Allocation_descriptor:
.quad _VN10Allocation_descriptor_0
.long 8
.long 0

_VN10Allocation_descriptor_0:
.ascii "Allocation"
.byte 0
.byte 1
.byte 2
.byte 0