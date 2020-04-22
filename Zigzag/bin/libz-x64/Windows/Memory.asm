[section .text]

extern _VirtualAlloc@16
extern _VirtualFree@12

global function_allocate:function
function_allocate:

push 0x04 ; PAGE_READWRITE
push 0x00001000 | 0x00002000
push dword [esp+8]
push 0

call _VirtualAlloc@16   

ret

global function_free:function
function_free:

push 0x00008000 ; MEM_RELEASE
push 0 ; dwSize
push dword [esp+12] ; lpAddress

call _VirtualFree@12  

ret

global function_copy:function
function_copy:

; Parameters
; esp+12: destination
; esp+8: count
; esp+4: source
; esp+0: return address

pop ebx
pop esi
pop ecx
pop edi

rep movsb

sub esp, 12

jmp ebx

global function_offset_copy:function
function_offset_copy:

; Parameters
; esp+16: offset
; esp+12: destination
; esp+8: count
; esp+4: source
; esp+0: return address

pop ebx
pop esi
pop ecx
pop edi
pop edx
add edi, edx

rep movsb

sub esp, 16

jmp ebx

global function_zero:function
function_zero:

; Parameters
; esp+8: count
; esp+4: destination
; esp+0: return address

pop ebx
pop edi
pop ecx

xor al, al

rep stosb
sub esp, 8

jmp ebx

global function_fill:function
function_fill:

; Parameters
; esp+12: value
; esp+8: count
; esp+4: destination
; esp+0: return address

pop ebx
pop edi
pop ecx
pop eax

rep stosb

sub esp, 12

jmp ebx