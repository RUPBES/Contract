class ActionMenu {

    constructor(options = {}) {

        this.selector = options.selector || ".action-btn"
        this.actions = options.actions || []

        this.menus = []
        this.context = null

    }

    init() {

        document.addEventListener("click", (e) => {

            const btn = e.target.closest(this.selector)

            if (btn) {

                e.stopPropagation()

                this.closeAll()

                this.context = { ...btn.dataset }

                this.createMenu(this.actions, btn, 0)

                return
            }

            this.closeAll()

        })

    }

    /* ========================= */
    /* TEMPLATE PARSER */
    /* ========================= */

    parseTemplate(str) {

        return str.replace(/\$\{(.*?)\}/g, (match, key) => {

            return this.context[key.trim()] ?? ""

        })

    }

    /* ========================= */
    /* MENU */
    /* ========================= */

    createMenu(actions, anchor, level) {

        this.closeFrom(level)

        const menu = document.createElement("div")
        menu.className = "action-menu"

        actions.forEach(action => {

            if (action.divider) {

                const div = document.createElement("div")
                div.className = "menu-divider"
                menu.appendChild(div)
                return

            }

            const item = document.createElement("div")
            item.className = "menu-item"

            if (action.danger)
                item.classList.add("danger")

            item.innerHTML = `
<span class="menu-icon">${action.icon || ""}</span>
<span class="menu-label">${action.label}</span>
${action.submenu ? '<span class="menu-arrow">›</span>' : ''}
`

            menu.appendChild(item)

            /* ========================= */
            /* SUBMENU */
            /* ========================= */

            if (action.submenu) {

                item.addEventListener("mouseenter", () => {
                    this.createMenu(action.submenu, item, level + 1)
                })

            }

            /* ========================= */
            /* ACTION */
            /* ========================= */

            else {

                item.addEventListener("mouseenter", () => {
                    this.closeFrom(level + 1)
                })

                item.addEventListener("click", () => {

                    /* LINK */

                    if (action.href) {

                        const url = this.parseTemplate(action.href)

                        window.location.href = url

                    }

                    /* CUSTOM HANDLER */

                    if (action.handler) {

                        action.handler(this.context)

                    }

                    this.closeAll()

                })

            }

        })

        document.body.appendChild(menu)

        const rect = anchor.getBoundingClientRect()

        let x, y

        if (level === 0) {

            x = rect.left
            y = rect.bottom + 6

        } else {

            x = rect.right + 4
            y = rect.top

        }

        menu.style.left = x + "px"
        menu.style.top = y + "px"

        this.menus[level] = menu

    }

    closeFrom(level) {

        for (let i = level; i < this.menus.length; i++) {
            this.menus[i]?.remove()
        }

        this.menus.length = level

    }

    closeAll() {
        this.closeFrom(0)
    }

}