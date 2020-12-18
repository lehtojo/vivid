.section .text
.intel_syntax noprefix
.global _VN6String7combineEPS__rS0_
_VN6String7combineEPS__rS0_:
push rbx
push rsi
push rdi
push rbp
sub rsp, 40
mov rbx, rcx
mov rsi, rdx
call _VN6String6lengthEv_rx
mov rcx, rsi
mov rdi, rax
call _VN6String6lengthEv_rx
lea rbp, [rax+1]
lea rcx, [rdi+rbp]
call _V8allocatex_rPh
mov rcx, [rbx+8]
mov rdx, rdi
mov r8, rax
mov rbx, rax
call _V4copyPhxS_
mov rcx, [rsi+8]
mov rdx, rbp
mov r8, rbx
mov r9, rdi
call _V11offset_copyPhxS_x
mov rcx, rbx
call _VN6String4initEPh_rPS_
add rsp, 40
pop rbp
pop rdi
pop rsi
pop rbx
ret

.global _VN6String6appendEh_rPS_
_VN6String6appendEh_rPS_:
push rbx
push rsi
push rdi
sub rsp, 32
mov rbx, rcx
mov rsi, rdx
call _VN6String6lengthEv_rx
lea rcx, [rax+2]
mov rdi, rax
call _V8allocatex_rPh
mov rcx, [rbx+8]
mov rdx, rdi
mov r8, rax
mov rbx, rax
call _V4copyPhxS_
mov byte ptr [rbx+rdi], sil
add rdi, 1
mov byte ptr [rbx+rdi], 0
mov rcx, rbx
call _VN6String4initEPh_rPS_
add rsp, 32
pop rdi
pop rsi
pop rbx
ret

.global _VN6String6insertExh_rPS_
_VN6String6insertExh_rPS_:
push rbx
push rsi
push rdi
push rbp
push r12
sub rsp, 32
mov rbx, rcx
mov rsi, rdx
mov rdi, r8
call _VN6String6lengthEv_rx
lea rcx, [rax+2]
mov rbp, rax
call _V8allocatex_rPh
mov rcx, [rbx+8]
mov rdx, rsi
mov r8, rax
mov r12, rax
call _V4copyPhxS_
mov rcx, rbp
sub rcx, rsi
lea r9, [rsi+1]
mov rdx, rcx
mov rcx, [rbx+8]
mov r8, r12
call _V11offset_copyPhxS_x
mov byte ptr [r12+rsi], dil
add rbp, 1
mov byte ptr [r12+rbp], 0
mov rcx, r12
call _VN6String4initEPh_rPS_
add rsp, 32
pop r12
pop rbp
pop rdi
pop rsi
pop rbx
ret

.global _VN6String4dataEv_rPh
_VN6String4dataEv_rPh:
mov rax, [rcx+8]
ret

.global _VN6String6lengthEv_rx
_VN6String6lengthEv_rx:
xor rax, rax
mov r8, [rcx+8]
movsx rdx, byte ptr [r8+rax]
test rdx, rdx
je _VN6String6lengthEv_rx_L1
_VN6String6lengthEv_rx_L0:
add rax, 1
mov r8, [rcx+8]
movsx rdx, byte ptr [r8+rax]
test rdx, rdx
jne _VN6String6lengthEv_rx_L0
_VN6String6lengthEv_rx_L1:
ret

.global _VN6String4initEPh_rPS_
_VN6String4initEPh_rPS_:
push rbx
sub rsp, 32
mov rbx, rcx
mov rcx, 16
call _V8allocatex_rPh
mov qword ptr [rax+8], rbx
add rsp, 32
pop rbx
ret

.global _V9to_stringx_rP6String
_V9to_stringx_rP6String:
push rbx
push rsi
push rdi
push rbp
sub rsp, 40
mov rbx, rcx
lea rcx, [rip+_V9to_stringx_rP6String_S0]
call _VN6String4initEPh_rPS_
lea rcx, [rip+_V9to_stringx_rP6String_S1]
mov rsi, rax
call _VN6String4initEPh_rPS_
mov rdi, rax
test rbx, rbx
jge _V9to_stringx_rP6String_L0
lea rcx, [rip+_V9to_stringx_rP6String_S2]
call _VN6String4initEPh_rPS_
mov rdi, rax
mov rcx, rbx
neg rcx
mov rbx, rcx
_V9to_stringx_rP6String_L0:
_V9to_stringx_rP6String_L3:
_V9to_stringx_rP6String_L2:
mov rax, rbx
cqo
mov rcx, 10
idiv rcx
mov rax, rbx
mov rbp, rdx
mov rcx, 1844674407370955162
mul rcx
mov rbx, rdx
sar rbx, 63
add rbx, rdx
mov r8, 48
add r8, rbp
mov rcx, rsi
xor rdx, rdx
call _VN6String6insertExh_rPS_
mov rsi, rax
test rbx, rbx
jne _V9to_stringx_rP6String_L5
mov rcx, rdi
mov rdx, rsi
call _VN6String7combineEPS__rS0_
add rsp, 40
pop rbp
pop rdi
pop rsi
pop rbx
ret
_V9to_stringx_rP6String_L5:
jmp _V9to_stringx_rP6String_L2
_V9to_stringx_rP6String_L4:
add rsp, 40
pop rbp
pop rdi
pop rsi
pop rbx
ret

.global _V9to_stringd_rP6String
_V9to_stringd_rP6String:
push rbx
push rsi
sub rsp, 40
cvttsd2si rcx, xmm0
movsd qword ptr [rsp+64], xmm0
call _V9to_stringx_rP6String
movsd xmm0, qword ptr [rsp+64]
pxor xmm1, xmm1
comisd xmm0, xmm1
jae _V9to_stringd_rP6String_L0
movsd xmm1, xmm0
xorpd xmm1, oword ptr [rip+_V9to_stringd_rP6String_C0]
movsd xmm0, xmm1
_V9to_stringd_rP6String_L0:
cvttsd2si rcx, xmm0
cvtsi2sd xmm1, rcx
subsd xmm0, xmm1
mov rbx, rax
pxor xmm1, xmm1
comisd xmm0, xmm1
jnz _V9to_stringd_rP6String_L2
lea rcx, [rip+_V9to_stringd_rP6String_S0]
movsd qword ptr [rsp+64], xmm0
call _VN6String4initEPh_rPS_
mov rcx, rbx
mov rdx, rax
call _VN6String7combineEPS__rS0_
add rsp, 40
pop rsi
pop rbx
ret
movsd xmm0, qword ptr [rsp+8]
_V9to_stringd_rP6String_L2:
mov rcx, rbx
mov rdx, 44
movsd qword ptr [rsp+64], xmm0
call _VN6String6appendEh_rPS_
mov rbx, rax
xor rsi, rsi
cmp rsi, 15
jge _V9to_stringd_rP6String_L5
movsd xmm0, qword ptr [rsp+64]
pxor xmm1, xmm1
comisd xmm0, xmm1
movsd qword ptr [rsp+64], xmm0
jbe _V9to_stringd_rP6String_L5
_V9to_stringd_rP6String_L4:
movsd xmm0, qword ptr [rsp+64]
movsd xmm1, qword ptr [rip+_V9to_stringd_rP6String_C1]
mulsd xmm0, xmm1
cvttsd2si rcx, xmm0
mov rdx, rcx
cvtsi2sd xmm1, rdx
subsd xmm0, xmm1
movsd xmm2, qword ptr [rip+_V9to_stringd_rP6String_C2]
addsd xmm2, xmm1
mov rcx, rbx
cvttsd2si rdx, xmm2
movsd qword ptr [rsp+64], xmm0
call _VN6String6appendEh_rPS_
mov rbx, rax
add rsi, 1
cmp rsi, 15
jge _V9to_stringd_rP6String_L8
movsd xmm0, qword ptr [rsp+64]
pxor xmm1, xmm1
comisd xmm0, xmm1
movsd qword ptr [rsp+64], xmm0
ja _V9to_stringd_rP6String_L4
_V9to_stringd_rP6String_L8:
_V9to_stringd_rP6String_L5:
mov rax, rbx
add rsp, 40
pop rsi
pop rbx
ret

.section .data

_VN6String_configuration:
.quad _VN6String_descriptor

_VN6String_descriptor:
.quad _VN6String_descriptor_0
.long 16
.long 0

_VN6String_descriptor_0:
.ascii "String"
.byte 0
.byte 1
.byte 2
.byte 0

.balign 16
_V9to_stringx_rP6String_S0:
.byte 0
.balign 16
_V9to_stringx_rP6String_S1:
.byte 0
.balign 16
_V9to_stringx_rP6String_S2:
.ascii "-"
.byte 0
.balign 16
_V9to_stringd_rP6String_S0:
.ascii ",0"
.byte 0

.balign 16
_V9to_stringd_rP6String_C0:
.byte 0, 0, 0, 0, 0, 0, 0, 128, 0, 0, 0, 0, 0, 0, 0, 128
.balign 16
_V9to_stringd_rP6String_C1:
.byte 0, 0, 0, 0, 0, 0, 36, 64 # 10.0
.balign 16
_V9to_stringd_rP6String_C2:
.byte 0, 0, 0, 0, 0, 0, 72, 64 # 48.0

