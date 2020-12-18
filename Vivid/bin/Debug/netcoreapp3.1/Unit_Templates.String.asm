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

.global _VN6String4plusEPS__rS0_
_VN6String4plusEPS__rS0_:
sub rsp, 40
call _VN6String7combineEPS__rS0_
add rsp, 40
ret

.global _VN6String3getEx_rh
_VN6String3getEx_rh:
mov r8, [rcx+8]
movsx rax, byte ptr [r8+rdx]
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

