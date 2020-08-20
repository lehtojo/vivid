section .text
global _start
_start:
call _V4initv_rx
mov rax, 60
xor rdi, rdi
syscall

extern _V8allocatex_rPh
extern _V4copyPhxPS_
extern _V11offset_copyPhxPS_x
extern _V14internal_printPhx

_V9fibonaccix:
push rbx
push rbp
push r12
push r13
push r14
sub rsp, 16
xor rbx, rbx
xor rbp, rbp
mov r12, 1
xor r13, r13
mov r14, rdi
cmp rbx, r14
jge _V9fibonaccix_L1
_V9fibonaccix_L0:
cmp rbx, 1
jg _V9fibonaccix_L3
mov rbp, rbx
jmp _V9fibonaccix_L2
_V9fibonaccix_L3:
lea rax, [r13+r12]
mov rcx, r12
mov r13, r12
mov r12, rax
mov rbp, rax
_V9fibonaccix_L2:
mov rdi, rbp
call _V9to_stringx_rP6String
mov rdi, rax
call _V8printslnP6String
add rbx, 1
cmp rbx, r14
jl _V9fibonaccix_L0
_V9fibonaccix_L1:
add rsp, 16
pop r14
pop r13
pop r12
pop rbp
pop rbx
ret

_V4initv_rx:
sub rsp, 8
mov rdi, 10
call _V9fibonaccix
xor rax, rax
add rsp, 8
ret

_V9to_stringx_rP6String:
push rbx
push rbp
push r12
push r13
sub rsp, 8
mov rcx, rdi
lea rdi, [rel _V9to_stringx_rP6String_S0]
mov rbx, rcx
call _VN6String4initEPh_rS0_
lea rdi, [rel _V9to_stringx_rP6String_S1]
mov rbp, rax
call _VN6String4initEPh_rS0_
test rbx, rbx
jge _V9to_stringx_rP6String_L0
lea rdi, [rel _V9to_stringx_rP6String_S2]
call _VN6String4initEPh_rS0_
neg rbx
_V9to_stringx_rP6String_L0:
mov r12, rax
_V9to_stringx_rP6String_L2:
mov rax, rbx
xor rdx, rdx
mov rcx, 10
idiv rcx
mov rax, rbx
mov rcx, rdx
mov rsi, 1844674407370955162
mul rsi
mov rbx, rdx
sar rbx, 63
add rbx, rdx
mov rdx, 48
add rdx, rcx
mov rdi, rbp
xor rsi, rsi
mov r8, rdx
mov rdx, r8
mov rbp, rcx
call _VN6String6insertExh_rPS_
test rbx, rbx
jne _V9to_stringx_rP6String_L3
mov rdi, r12
mov rsi, rax
mov r13, rax
call _VN6String7combineEPS__rS0_
add rsp, 8
pop r13
pop r12
pop rbp
pop rbx
ret
mov rax, r13
_V9to_stringx_rP6String_L3:
mov rbp, rax
jmp _V9to_stringx_rP6String_L2
add rsp, 8
pop r13
pop r12
pop rbp
pop rbx
ret

_V8printslnP6String:
push rbx
sub rsp, 16
mov rsi, 10
mov rbx, rdi
call _VN6String6appendEh_rPS_
mov rdi, rax
call _VN6String4dataEv_rPh
mov rdi, rbx
mov rbx, rax
call _VN6String6lengthEv_rx
add rax, 1
mov rdi, rbx
mov rsi, rax
call _V14internal_printPhx
add rsp, 16
pop rbx
ret

_VN6String4initEPh_rS0_:
push rbx
sub rsp, 16
mov rcx, rdi
mov rdi, 8
mov rbx, rcx
call _V8allocatex_rPh
mov qword [rax], rbx
add rsp, 16
pop rbx
ret

_VN6String7combineEPS__rS0_:
push rbx
push rbp
push r12
push r13
sub rsp, 8
mov rbx, rsi
mov rbp, rdi
call _VN6String6lengthEv_rx
mov rdi, rbx
mov r12, rax
call _VN6String6lengthEv_rx
add rax, 1
lea rdi, [r12+rax]
mov r13, rax
call _V8allocatex_rPh
mov rdi, [rbp]
mov rsi, r12
mov rdx, rax
mov rbp, rax
call _V4copyPhxPS_
mov rdi, [rbx]
mov rsi, r13
mov rdx, rbp
mov rcx, r12
call _V11offset_copyPhxPS_x
mov rdi, rbp
call _VN6String4initEPh_rS0_
add rsp, 8
pop r13
pop r12
pop rbp
pop rbx
ret

_VN6String6appendEh_rPS_:
push rbx
push rbp
push r12
sub rsp, 16
mov rbx, rsi
mov rbp, rdi
call _VN6String6lengthEv_rx
lea rdi, [rax+2]
mov r12, rax
call _V8allocatex_rPh
mov rdi, [rbp]
mov rsi, r12
mov rdx, rax
mov rbp, rax
call _V4copyPhxPS_
mov byte [rbp+r12], bl
add r12, 1
mov byte [rbp+r12], 0
mov rdi, rbp
call _VN6String4initEPh_rS0_
add rsp, 16
pop r12
pop rbp
pop rbx
ret

_VN6String6insertExh_rPS_:
push rbx
push rbp
push r12
push r13
push r14
sub rsp, 16
mov rbx, rdx
mov rbp, rsi
mov r12, rdi
call _VN6String6lengthEv_rx
lea rdi, [rax+2]
mov r13, rax
call _V8allocatex_rPh
mov rdi, [r12]
mov rsi, rbp
mov rdx, rax
mov r14, rax
call _V4copyPhxPS_
mov rcx, r13
sub rcx, rbp
lea rdx, [rbp+1]
mov rdi, [r12]
mov rsi, rcx
mov rcx, rdx
mov rdx, r14
call _V11offset_copyPhxPS_x
mov byte [r14+rbp], bl
add r13, 1
mov byte [r14+r13], 0
mov rdi, r14
call _VN6String4initEPh_rS0_
add rsp, 16
pop r14
pop r13
pop r12
pop rbp
pop rbx
ret

_VN6String4dataEv_rPh:
mov rax, [rdi]
ret

_VN6String6lengthEv_rx:
xor rax, rax
mov rdx, [rdi]
movzx rcx, byte [rdx+rax]
test rcx, rcx
je _VN6String6lengthEv_rx_L1
_VN6String6lengthEv_rx_L0:
add rax, 1
mov rdx, [rdi]
movzx rcx, byte [rdx+rax]
test rcx, rcx
jne _VN6String6lengthEv_rx_L0
_VN6String6lengthEv_rx_L1:
ret

section .data

_V9to_stringx_rP6String_S0 db '', 0
_V9to_stringx_rP6String_S1 db '', 0
_V9to_stringx_rP6String_S2 db '-', 0