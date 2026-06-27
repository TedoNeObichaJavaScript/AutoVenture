// AutoVenture — front-end interactions.
// GSAP drives entrance/stagger/modal/shuffle; a single fetch() wires the
// booking modal to POST /Rentals/Book. Everything degrades gracefully:
// without GSAP the page is fully usable, just without motion.

(function () {
    'use strict';

    const hasGsap = typeof window.gsap !== 'undefined';
    if (hasGsap) document.documentElement.classList.add('js-anim');
    const reduced = window.matchMedia('(prefers-reduced-motion: reduce)').matches;

    document.addEventListener('DOMContentLoaded', () => {
        initReveals();
        initFleetFilter();
        initBookingForm();
    });

    /* ---------- entrance + scroll reveals ---------- */
    function initReveals() {
        if (!hasGsap || reduced) {
            document.querySelectorAll('[data-reveal]').forEach(el => (el.style.opacity = 1));
            return;
        }
        // Hero block first, then any staggered groups.
        const hero = document.querySelectorAll('[data-reveal="hero"]');
        if (hero.length) {
            gsap.from(hero, {
                y: 28, opacity: 0, duration: 0.7, ease: 'power3.out', stagger: 0.08,
                onStart: () => hero.forEach(el => (el.style.opacity = 1)),
            });
        }
        const grid = document.querySelectorAll('[data-reveal="card"]');
        if (grid.length) {
            gsap.to(grid, { opacity: 1, duration: 0.01 });
            gsap.from(grid, {
                y: 40, opacity: 0, duration: 0.5, ease: 'back.out(1.4)', stagger: 0.06, delay: 0.15,
            });
        }
    }

    /* ---------- fleet filter / sort with Flip shuffle ---------- */
    function initFleetFilter() {
        const grid = document.getElementById('fleet-grid');
        if (!grid) return;
        const cards = Array.from(grid.children);
        const search = document.getElementById('fleet-search');
        const sort = document.getElementById('fleet-sort');
        const canFlip = hasGsap && typeof window.Flip !== 'undefined' && !reduced;

        function apply() {
            const q = (search?.value || '').trim().toLowerCase();
            const state = canFlip ? Flip.getState(cards) : null;

            // sort
            const sorted = cards.slice().sort((a, b) => {
                const pa = +a.dataset.price, pb = +b.dataset.price;
                const na = a.dataset.name.toLowerCase(), nb = b.dataset.name.toLowerCase();
                switch (sort?.value) {
                    case 'price-asc': return pa - pb;
                    case 'price-desc': return pb - pa;
                    case 'name': return na < nb ? -1 : na > nb ? 1 : 0;
                    default: return 0;
                }
            });
            sorted.forEach(c => grid.appendChild(c));

            // filter
            cards.forEach(c => {
                const match = c.dataset.name.toLowerCase().includes(q);
                c.style.display = match ? '' : 'none';
            });

            if (canFlip && state) {
                Flip.from(state, { duration: 0.45, ease: 'power2.inOut', scale: true, absolute: true });
            }
        }

        search?.addEventListener('input', apply);
        sort?.addEventListener('change', apply);
    }

    /* ---------- booking modal ---------- */
    const modal = () => document.getElementById('booking-modal');

    window.openBookingModal = function (btn) {
        const m = modal();
        if (!m) return;
        const d = btn.dataset;
        m.querySelector('#bm-carid').value = d.carId || '';
        m.querySelector('#bm-title').textContent = d.carName || 'Car';
        m.querySelector('#bm-price').textContent = '$' + (d.carPrice || '0');
        const img = m.querySelector('#bm-img');
        if (d.carImg) { img.src = d.carImg; img.alt = d.carName || ''; img.style.display = ''; }
        else { img.style.display = 'none'; }

        resetBookingState();
        m.classList.remove('hidden');
        m.classList.add('flex');
        if (hasGsap && !reduced) {
            gsap.fromTo(m.querySelector('[data-modal-panel]'),
                { y: 30, scale: 0.96, opacity: 0 },
                { y: 0, scale: 1, opacity: 1, duration: 0.3, ease: 'back.out(1.6)' });
        }
    };

    window.closeBookingModal = function () {
        const m = modal();
        if (!m) return;
        m.classList.add('hidden');
        m.classList.remove('flex');
    };

    function resetBookingState() {
        const form = document.getElementById('booking-form');
        form?.reset();
        toggle('#bm-errors', false);
        toggle('#bm-success', false);
        const submit = document.getElementById('bm-submit');
        if (submit) { submit.disabled = false; submit.textContent = 'Confirm booking'; }
    }

    function initBookingForm() {
        const form = document.getElementById('booking-form');
        if (!form) return;

        form.addEventListener('submit', async (e) => {
            e.preventDefault();
            const submit = document.getElementById('bm-submit');
            const data = Object.fromEntries(new FormData(form).entries());
            data.CarId = parseInt(data.CarId, 10);

            submit.disabled = true;
            submit.textContent = 'Booking…';
            toggle('#bm-errors', false);

            try {
                const res = await fetch('/Rentals/Book', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(data),
                });
                const body = await res.json().catch(() => ({}));

                if (res.ok) {
                    document.getElementById('bm-total').textContent = (body.total ?? 0);
                    form.querySelectorAll('input,button[type=submit]').forEach(el => (el.style.display = 'none'));
                    toggle('#bm-success', true);
                    if (hasGsap && !reduced) {
                        gsap.fromTo('#bm-success', { scale: 0.8, opacity: 0 },
                            { scale: 1, opacity: 1, duration: 0.4, ease: 'back.out(2)' });
                    }
                } else {
                    showErrors(body);
                    submit.disabled = false;
                    submit.textContent = 'Confirm booking';
                }
            } catch {
                showErrors({ message: 'Network error — please try again.' });
                submit.disabled = false;
                submit.textContent = 'Confirm booking';
            }
        });
    }

    function showErrors(body) {
        const ul = document.getElementById('bm-errors');
        if (!ul) return;
        const items = (body.errors && body.errors.length)
            ? body.errors.map(e => e.errorMessage || e.ErrorMessage)
            : [body.message || 'Validation failed.'];
        ul.innerHTML = items.map(t => `<li>${t}</li>`).join('');
        toggle('#bm-errors', true);
        if (hasGsap && !reduced) {
            gsap.fromTo(ul, { x: -6 }, { x: 0, duration: 0.3, ease: 'elastic.out(1,0.4)' });
        }
    }

    function toggle(sel, show) {
        const el = document.querySelector(sel);
        if (el) el.classList.toggle('hidden', !show);
    }
})();
