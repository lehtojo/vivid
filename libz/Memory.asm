[section .text]

global function_allocate:function
function_allocate:

;   System call parameters
;
;   off_t offset,
;   int fd, 
;   int flags,    
;   int prot, 
;   size_t length, 
;   void *addr
;
push dword 0
push dword -1
push dword 0x22 ; HARDWARE_MEMORY | VISIBILITY_PRIVATE
push dword 0x03 ; PERMISSION_READ | PERMISSION_WRITE
push dword [esp+20] ; Parameter: Region size
push dword 0

; push qword 0xFFFFFFFF ; 0 | -1
; push qword 0x0000002200000003 ; HARDWARE_MEMORY | VISIBILITY_PRIVATE + PERMISSION_READ | PERMISSION_WRITE
; push dword [esp+16] ; Parameter: Region size
; push dword 0

; System call: mmap
mov eax, 0x5a
mov ebx, esp
int 0x80

; Cleanup
add esp, 24
ret

global function_copy_0:function
function_copy_0:

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

global function_copy_1:function
function_copy_1:

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

rep stos

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

rep stos

sub esp, 12

jmp ebx