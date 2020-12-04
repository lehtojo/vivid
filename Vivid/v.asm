.section .text
.intel_syntax noprefix
.file 1 "Sandbox.v"
.global main
main:
jmp _V4initv_rx

.extern _V17internal_allocatex_rPh

_V4initv_rx:
.loc 1 15 1
.cfi_startproc
push rbp
.cfi_def_cfa_offset 16
.cfi_offset 6, -16
mov rbp, rsp
.cfi_def_cfa_register 6
sub rsp, 80
.loc 1 16 6
.loc 1 16 8
call _VN3Goo4initEv_rPS_
mov qword ptr [rsp+72], rax
.loc 1 17 4
movsd xmm0, qword ptr [rip+_V4initv_rx_C0]
movsd qword ptr [rsp+64], xmm0
.loc 1 19 4
mov qword ptr [rsp+56], 1
.loc 1 20 4
mov qword ptr [rsp+48], 2
.loc 1 22 2
.loc 1 22 10
mov qword ptr [rsp+40], 0
mov rcx, [rsp+40]
cmp rcx, 10
jge _V4initv_rx_L1
_V4initv_rx_L0:
.loc 1 23 3
mov rcx, [rsp+56]
add rcx, 1
.loc 1 22 23
mov rdx, [rsp+40]
add rdx, 1
mov qword ptr [rsp+56], rcx
mov qword ptr [rsp+40], rdx
mov rcx, [rsp+40]
cmp rcx, 10
mov qword ptr [rsp+40], rcx
jl _V4initv_rx_L0
_V4initv_rx_L1:
.loc 1 26 4
.loc 1 26 10
mov rdx, [rsp+56]
add rdx, [rsp+48]
mov rcx, [rsp+72]
call _VN3Goo3fooEx_rx
mov qword ptr [rsp+48], rax
mov rax, [rsp+56]
mov rax, [rsp+48]
.loc 1 28 2
mov rcx, [rsp+56]
mov rdx, [rsp+48]
cmp rcx, rdx
jle _V4initv_rx_L5
.loc 1 29 5
mov qword ptr [rsp+56], 10
jmp _V4initv_rx_L4
_V4initv_rx_L5:
.loc 1 31 2
.loc 1 32 5
mov qword ptr [rsp+48], 5
_V4initv_rx_L4:
.loc 1 35 2
mov rax, [rsp+56]
add rax, [rsp+48]
add rsp, 80
.cfi_def_cfa 7, 8
pop rbp
ret
_V4initv_rx_end:
.cfi_endproc

_V8allocatex_rPh:
.loc 1 23 1
.cfi_startproc
push rbx
push rbp
.cfi_def_cfa_offset 16
.cfi_offset 6, -16
mov rbp, rsp
.cfi_def_cfa_register 6
sub rsp, 56
.loc 1 24 5
mov rdx, [rip+_VN10Allocation_current]
test rdx, rdx
je _V8allocatex_rPh_L0
mov r8, [rip+_VN10Allocation_current]
mov rdx, [r8+16]
add rdx, rcx
cmp rdx, 1000000
jg _V8allocatex_rPh_L0
.loc 1 25 18
mov r8, [rip+_VN10Allocation_current]
mov rdx, [r8+16]
mov qword ptr [rsp+48], rdx
.loc 1 26 37
mov rdx, [rip+_VN10Allocation_current]
add qword ptr [rdx+16], rcx
.loc 1 28 9
mov rdx, [rip+_VN10Allocation_current]
mov rax, [rdx+8]
add rax, [rsp+48]
add rsp, 56
.cfi_def_cfa 7, 8
pop rbp
pop rbx
ret
_V8allocatex_rPh_L0:
.loc 1 31 13
.loc 1 31 15
mov rbx, rcx
mov rcx, 1000000
call _V17internal_allocatex_rPh
mov qword ptr [rsp+40], rax
.loc 1 33 10
.loc 1 33 12
mov rcx, 24
call _V17internal_allocatex_rPh
mov qword ptr [rsp+32], rax
.loc 1 34 18
mov rcx, [rsp+32]
mov rdx, [rsp+40]
mov qword ptr [rcx+8], rdx
.loc 1 35 19
mov rcx, [rsp+32]
mov qword ptr [rcx+16], rbx
.loc 1 37 24
mov rcx, [rsp+32]
mov qword ptr [rip+_VN10Allocation_current], rcx
.loc 1 39 5
mov rax, [rsp+40]
add rsp, 56
.cfi_def_cfa 7, 8
pop rbp
pop rbx
ret
_V8allocatex_rPh_end:
.cfi_endproc

_V8inheritsPhS__rx:
.loc 1 46 1
.cfi_startproc
push rbp
.cfi_def_cfa_offset 16
.cfi_offset 6, -16
mov rbp, rsp
.cfi_def_cfa_register 6
sub rsp, 56
.loc 1 47 4
mov r8, [rcx]
mov qword ptr [rsp+48], r8
.loc 1 48 4
mov r8, [rdx]
mov qword ptr [rsp+40], r8
.loc 1 50 4
mov r9, [rsp+40]
movzx r8, byte ptr [r9]
mov qword ptr [rsp+32], r8
.loc 1 51 4
mov qword ptr [rsp+24], 0
.loc 1 53 2
_V8inheritsPhS__rx_L1:
_V8inheritsPhS__rx_L0:
.loc 1 54 5
mov rdx, [rsp+48]
mov r8, [rsp+24]
movzx rcx, byte ptr [rdx+r8]
mov qword ptr [rsp+16], rcx
.loc 1 55 3
add r8, 1
mov rax, [rsp+16]
mov rax, [rsp+32]
mov rcx, [rsp+40]
.loc 1 57 3
mov r9, [rsp+16]
cmp r9, rax
jnz _V8inheritsPhS__rx_L4
.loc 1 58 6
mov qword ptr [rsp+8], 1
.loc 1 60 4
_V8inheritsPhS__rx_L7:
_V8inheritsPhS__rx_L6:
.loc 1 61 7
movzx r9, byte ptr [rdx+r8]
mov qword ptr [rsp+16], r9
.loc 1 62 7
mov r10, [rsp+8]
movzx r9, byte ptr [rcx+r10]
mov qword ptr [rsp], r9
.loc 1 64 5
add r8, 1
.loc 1 65 5
add r10, 1
mov r9, [rsp+16]
mov r11, [rsp]
.loc 1 67 5
mov r11, [rsp]
cmp r9, r11
jz _V8inheritsPhS__rx_L9
cmp r9, 1
jne _V8inheritsPhS__rx_L9
mov r11, [rsp]
test r11, r11
jne _V8inheritsPhS__rx_L9
.loc 1 67 72
mov rax, 1
add rsp, 56
.cfi_def_cfa 7, 8
pop rbp
ret
_V8inheritsPhS__rx_L9:
mov qword ptr [rsp+8], r10
mov qword ptr [rsp+16], r9
jmp _V8inheritsPhS__rx_L6
_V8inheritsPhS__rx_L8:
jmp _V8inheritsPhS__rx_L3
_V8inheritsPhS__rx_L4:
mov r9, [rsp+16]
cmp r9, 2
jne _V8inheritsPhS__rx_L3
.loc 1 70 42
xor rax, rax
add rsp, 56
.cfi_def_cfa 7, 8
pop rbp
ret
_V8inheritsPhS__rx_L3:
mov qword ptr [rsp+48], rdx
mov qword ptr [rsp+24], r8
mov qword ptr [rsp+32], rax
mov qword ptr [rsp+40], rcx
jmp _V8inheritsPhS__rx_L0
_V8inheritsPhS__rx_end:
_V8inheritsPhS__rx_L2:
add rsp, 56
.cfi_def_cfa 7, 8
pop rbp
ret
.cfi_endproc

_VN3Goo4initEv_rPS_:
.loc 1 5 2
.cfi_startproc
push rbp
.cfi_def_cfa_offset 16
.cfi_offset 6, -16
mov rbp, rsp
.cfi_def_cfa_register 6
sub rsp, 48
mov rcx, 24
call _V8allocatex_rPh
mov qword ptr [rsp+40], rax
.loc 1 6 5
mov rcx, [rsp+40]
mov qword ptr [rcx+8], 1
.loc 1 7 5
mov rcx, [rsp+40]
mov qword ptr [rcx+16], -1
mov rax, [rsp+40]
add rsp, 48
.cfi_def_cfa 7, 8
pop rbp
ret
_VN3Goo4initEv_rPS__end:
.cfi_endproc

_VN3Goo3fooEx_rx:
.loc 1 10 2
.cfi_startproc
push rbp
.cfi_def_cfa_offset 16
.cfi_offset 6, -16
mov rbp, rsp
.cfi_def_cfa_register 6
sub rsp, 8
.loc 1 11 3
mov rax, [rcx+8]
mov r8, [rcx+16]
add rax, r8
add rax, rdx
add rsp, 8
.cfi_def_cfa 7, 8
pop rbp
ret
_VN3Goo3fooEx_rx_end:
.cfi_endproc

end:
.section .data

_VN10Allocation_current:
.quad 0

_VN3Goo_configuration:
.quad _VN3Goo_descriptor

_VN3Goo_descriptor:
.quad _VN3Goo_descriptor_0
.long 24
.long 0

_VN3Goo_descriptor_0:
.ascii "Goo"
.byte 0
.byte 1
.byte 2
.byte 0

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

.balign 16
_V4initv_rx_C0:
.byte 84, 227, 165, 155, 196, 32, 9, 64 # 3.141

.section .debug_abbrev
.byte 1
.byte 17
.byte 1
.byte 37
.byte 8
.byte 19
.byte 5
.byte 3
.byte 8
.byte 16
.byte 23
.byte 27
.byte 8
.byte 17
.byte 1
.byte 18
.byte 6
.byte 0
.byte 0
.byte 2
.byte 2
.byte 1
.byte 54
.byte 11
.byte 3
.byte 8
.byte 11
.byte 6
.byte 58
.byte 6
.byte 59
.byte 6
.byte 0
.byte 0
.byte 3
.byte 36
.byte 0
.byte 3
.byte 8
.byte 62
.byte 11
.byte 11
.byte 6
.byte 0
.byte 0
.byte 4
.byte 15
.byte 0
.byte 73
.byte 19
.byte 0
.byte 0
.byte 5
.byte 13
.byte 0
.byte 3
.byte 8
.byte 73
.byte 19
.byte 58
.byte 6
.byte 59
.byte 6
.byte 56
.byte 6
.byte 50
.byte 11
.byte 0
.byte 0
.byte 6
.byte 52
.byte 0
.byte 2
.byte 24
.byte 3
.byte 8
.byte 58
.byte 6
.byte 59
.byte 6
.byte 73
.byte 19
.byte 0
.byte 0
.byte 7
.byte 46
.byte 1
.byte 17
.byte 1
.byte 18
.byte 6
.byte 64
.byte 24
.byte 3
.byte 8
.byte 58
.byte 6
.byte 59
.byte 6
.byte 73
.byte 6
.byte 0
.byte 0
.byte 8
.byte 46
.byte 1
.byte 17
.byte 1
.byte 18
.byte 6
.byte 64
.byte 24
.byte 3
.byte 8
.byte 58
.byte 6
.byte 59
.byte 6
.byte 73
.byte 6
.byte 0
.byte 0
.byte 9
.byte 46
.byte 1
.byte 17
.byte 1
.byte 18
.byte 6
.byte 64
.byte 24
.byte 3
.byte 8
.byte 58
.byte 6
.byte 59
.byte 6
.byte 73
.byte 6
.byte 0
.byte 0
.byte 0

.section .debug_info
debug_info_start:
.long debug_info_end - debug_info_version
debug_info_version:
.short 4
.secrel32 .debug_abbrev
.byte 8
.byte 1
.ascii "Vivid version 1.0"
.byte 0
.short 30583
.ascii "Sandbox.v"
.byte 0
.secrel32 .debug_line_start
.ascii "C:/Users/joona/vivid/Vivid/Examples"
.byte 0
.quad main
.long end - main
.byte 7
.quad _V4initv_rx
.long _V4initv_rx_end - _V4initv_rx
.byte 1
.byte 86
.ascii "_V4initv_rx"
.byte 0
.long 1
.long 15
.long _VNx_debug - debug_info_start
.byte 6
.byte 2
.byte 145
.byte 120
.ascii "goo"
.byte 0
.long 1
.long 16
.long _VN3Goo_pointer_debug - debug_info_start
.byte 6
.byte 2
.byte 145
.byte 112
.ascii "g"
.byte 0
.long 1
.long 17
.long _VNd_debug - debug_info_start
.byte 6
.byte 2
.byte 145
.byte 104
.ascii "a"
.byte 0
.long 1
.long 19
.long _VNx_debug - debug_info_start
.byte 6
.byte 2
.byte 145
.byte 96
.ascii "b"
.byte 0
.long 1
.long 20
.long _VNx_debug - debug_info_start
.byte 0
.byte 8
.quad _V8allocatex_rPh
.long _V8allocatex_rPh_end - _V8allocatex_rPh
.byte 1
.byte 86
.ascii "_V8allocatex_rPh"
.byte 0
.long 1
.long 23
.long _VNPh_debug - debug_info_start
.byte 6
.byte 2
.byte 145
.byte 104
.ascii "address"
.byte 0
.long 1
.long 31
.long _VNPh_debug - debug_info_start
.byte 6
.byte 2
.byte 145
.byte 96
.ascii "page"
.byte 0
.long 1
.long 33
.long _VN4Page_pointer_debug - debug_info_start
.byte 0
.byte 9
.quad _V8inheritsPhS__rx
.long _V8inheritsPhS__rx_end - _V8inheritsPhS__rx
.byte 1
.byte 86
.ascii "_V8inheritsPhS__rx"
.byte 0
.long 1
.long 46
.long _VNx_debug - debug_info_start
.byte 6
.byte 2
.byte 145
.byte 120
.ascii "x"
.byte 0
.long 1
.long 47
.long _VNPh_debug - debug_info_start
.byte 6
.byte 2
.byte 145
.byte 112
.ascii "y"
.byte 0
.long 1
.long 48
.long _VNPh_debug - debug_info_start
.byte 6
.byte 2
.byte 145
.byte 104
.ascii "s"
.byte 0
.long 1
.long 50
.long _VNPh_debug - debug_info_start
.byte 6
.byte 2
.byte 145
.byte 96
.ascii "i"
.byte 0
.long 1
.long 51
.long _VNx_debug - debug_info_start
.byte 0
_VNb_debug:
.byte 3
.ascii "bool"
.byte 0
.byte 2
.long 1
_VNPh_debug:
.byte 3
.ascii "link"
.byte 0
.byte 1
.long 8
_VNc_debug:
.byte 3
.ascii "tiny"
.byte 0
.byte 6
.long 1
_VNs_debug:
.byte 3
.ascii "small"
.byte 0
.byte 5
.long 2
_VNi_debug:
.byte 3
.ascii "normal"
.byte 0
.byte 5
.long 4
_VNx_debug:
.byte 3
.ascii "large"
.byte 0
.byte 5
.long 8
_VNh_debug:
.byte 3
.ascii "u8"
.byte 0
.byte 8
.long 1
_VNt_debug:
.byte 3
.ascii "u16"
.byte 0
.byte 7
.long 2
_VNj_debug:
.byte 3
.ascii "u32"
.byte 0
.byte 7
.long 4
_VNy_debug:
.byte 3
.ascii "u64"
.byte 0
.byte 7
.long 8
_VNd_debug:
.byte 3
.ascii "decimal"
.byte 0
.byte 4
.long 8
_VN3Goo_debug:
.byte 2
.byte 4
.ascii "Goo"
.byte 0
.long 24
.long 1
.long 1
.byte 5
.ascii "x"
.byte 0
.long _VNx_debug - debug_info_start
.long 1
.long 2
.long 8
.byte 1
.byte 5
.ascii "y"
.byte 0
.long _VNx_debug - debug_info_start
.long 1
.long 3
.long 16
.byte 1
_VN3Goo_pointer_debug:
.byte 4
.long _VN3Goo_debug - debug_info_start
_VN4Page_debug:
.byte 2
.byte 4
.ascii "Page"
.byte 0
.long 24
.long 1
.long 11
.byte 5
.ascii "address"
.byte 0
.long _VNPh_debug - debug_info_start
.long 1
.long 12
.long 8
.byte 1
.byte 5
.ascii "position"
.byte 0
.long _VNx_debug - debug_info_start
.long 1
.long 13
.long 16
.byte 1
_VN4Page_pointer_debug:
.byte 4
.long _VN4Page_debug - debug_info_start
_VN10Allocation_debug:
.byte 2
.byte 4
.ascii "Allocation"
.byte 0
.long 8
.long 1
.long 16
.byte 5
.ascii "current"
.byte 0
.long _VN4Page_pointer_debug - debug_info_start
.long 1
.long 18
.long 8
.byte 1
_VN10Allocation_pointer_debug:
.byte 4
.long _VN10Allocation_debug - debug_info_start
.byte 0
debug_info_end:

.section .debug_str

.section .debug_line
.debug_line_start: