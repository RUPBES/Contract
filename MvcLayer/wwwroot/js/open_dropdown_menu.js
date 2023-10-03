document.querySelectorAll('.dropdown-toggle').forEach((e) => {

    e.addEventListener('click', () => {

        let menu = e.parentNode.querySelector('div.dropdown-menu');
        
        
        if (menu.classList.contains('show')){
            menu.classList.remove('show');
            e.classList.remove('active');
        }
        else {
            unactiveMenuDropdown();
            menu.classList.add('show');
            e.classList.add('active');
        }
    });
});

document.addEventListener('click', (e) => {   
    if (!e.target.matches('.active')) {
        unactiveMenuDropdown();
    }
});

function unactiveMenuDropdown() {
    document.querySelectorAll('.dropdown-menu.show').forEach((ee) => {
        ee.classList.remove('show');
    });

    document.querySelectorAll('.dropdown-toggle.active').forEach((ee) => {
        ee.classList.remove('active');
    });
}