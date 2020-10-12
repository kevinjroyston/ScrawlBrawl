The shared module contains classes and resources which are used in more than one dynamically loaded module. By always loading with the application the shared components are ready whenever a module requests them.

https://angular-folder-structure.readthedocs.io/en/latest/shared.html

Do not provide any services in here, as SharedModule may be instantiated more than once.