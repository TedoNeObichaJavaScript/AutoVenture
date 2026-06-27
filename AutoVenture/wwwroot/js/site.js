// AutoVenture — front-end interactions.
// GSAP drives entrance/stagger/modal/shuffle + graffiti paint-splat bursts; a
// single fetch() wires the booking modal to POST /Rentals/Book. Everything
// degrades gracefully: without GSAP the page is fully usable and visible.

(function () {
    'use strict';

    const hasGsap = typeof window.gsap !== 'undefined';
    if (hasGsap) document.documentElement.classList.add('js-anim');
    const reduced = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    const SPLAT_COLORS = ['#d6f532', '#2f5cff', '#ff5a3c', '#0a0a0a', '#ffffff'];

    document.addEventListener('DOMContentLoaded', () => {
        initReveals();
        initFleetFilter();
        initBookingForm();
        // Safety net: whatever happens with GSAP, nothing stays invisible.
        setTimeout(forceVisible, 1400);
    });

    function forceVisible() {
        document.querySelectorAll('[data-reveal]').forEach(el => {
            if (parseFloat(getComputedStyle(el).opacity) < 0.99) el.style.opacity = 1;
        });
    }

    /* ---------- entrance reveals (energetic) ---------- */
    function initReveals() {
        const all = document.querySelectorAll('[data-reveal]');
        if (!hasGsap || reduced) { all.forEach(el => (el.style.opacity = 1)); return; }

        // Hero: punchy pop-in with a touch of rotation.
        const hero = document.querySelectorAll('[data-reveal="hero"]');
        if (hero.length) {
            gsap.fromTo(hero,
                { opacity: 0, y: 34, rotate: -2, scale: 0.96 },
                { opacity: 1, y: 0, rotate: 0, scale: 1, duration: 0.7,
                  ease: 'back.out(1.7)', stagger: 0.09, clearProps: 'transform' });
        }

        // Cards: graffiti-sticker slap-down — overshoot scale + skew, staggered.
        const cards = document.querySelectorAll('[data-reveal="card"]');
        if (cards.length) {
            gsap.fromTo(cards,
                { opacity: 0, y: 60, scale: 0.8, rotate: (i) => (i % 2 ? 5 : -5) },
                { opacity: 1, y: 0, scale: 1, rotate: 0, duration: 0.55,
                  ease: 'back.out(2)', stagger: { each: 0.07, from: 'start' },
                  delay: 0.15, clearProps: 'transform' });
        }

        // Decorative graffiti splats bloom in behind the hero.
        const blobs = document.querySelectorAll('[data-splat-deco]');
        if (blobs.length) {
            gsap.fromTo(blobs,
                { scale: 0, rotate: -40, opacity: 0 },
                { scale: 1, rotate: 0, opacity: 1, duration: 0.9,
                  ease: 'elastic.out(1, 0.5)', stagger: 0.12, delay: 0.2 });
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

            cards.forEach(c => {
                const match = c.dataset.name.toLowerCase().includes(q);
                c.style.display = match ? '' : 'none';
                c.style.opacity = 1; // never let filtered cards strand invisible
            });

            if (canFlip && state) {
                Flip.from(state, { duration: 0.5, ease: 'power3.inOut', scale: true,
                    absolute: true, stagger: 0.03 });
            }
        }

        search?.addEventListener('input', apply);
        sort?.addEventListener('change', apply);
    }

    /* ---------- graffiti paint-splat burst ---------- */
    function splatBurst(x, y, count) {
        if (!hasGsap || reduced) return;
        count = count || 14;
        const layer = document.createElement('div');
        layer.style.cssText = 'position:fixed;left:0;top:0;width:0;height:0;z-index:60;pointer-events:none;';
        document.body.appendChild(layer);

        for (let i = 0; i < count; i++) {
            const dot = document.createElement('span');
            const size = 8 + Math.floor(Math.abs(Math.sin(i * 12.9898) * 9999) % 18);
            const color = SPLAT_COLORS[i % SPLAT_COLORS.length];
            dot.style.cssText =
                `position:absolute;left:${x}px;top:${y}px;width:${size}px;height:${size}px;` +
                `background:${color};border:2px solid #0a0a0a;border-radius:50% 50% 50% 0;` +
                `transform:translate(-50%,-50%);`;
            layer.appendChild(dot);

            const ang = (i / count) * Math.PI * 2 + i;
            const dist = 60 + (i % 5) * 26;
            gsap.to(dot, {
                x: Math.cos(ang) * dist,
                y: Math.sin(ang) * dist,
                rotate: (i % 2 ? 1 : -1) * 180,
                scale: 0.2,
                opacity: 0,
                duration: 0.6 + (i % 4) * 0.12,
                ease: 'power3.out',
            });
        }
        gsap.delayedCall(1.3, () => layer.remove());
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

        // Paint-splat where the user clicked.
        const r = btn.getBoundingClientRect();
        splatBurst(r.left + r.width / 2, r.top + r.height / 2, 16);

        resetBookingState();
        m.classList.remove('hidden');
        m.classList.add('flex');
        if (hasGsap && !reduced) {
            gsap.fromTo(m.querySelector('[data-modal-panel]'),
                { y: 40, scale: 0.9, rotate: -3, opacity: 0 },
                { y: 0, scale: 1, rotate: 0, opacity: 1, duration: 0.4, ease: 'back.out(2)' });
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
        form?.querySelectorAll('input,button[type=submit]').forEach(el => (el.style.display = ''));
        toggle('#bm-errors', false);
        toggle('#bm-success', false);
        const submit = document.getElementById('bm-submit');
        if (submit) { submit.disabled = false; submit.textContent = 'Confirm booking'; submit.style.display = ''; }
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
                        gsap.fromTo('#bm-success', { scale: 0.7, rotate: -4, opacity: 0 },
                            { scale: 1, rotate: 0, opacity: 1, duration: 0.45, ease: 'back.out(2.2)' });
                        const panel = document.querySelector('[data-modal-panel]').getBoundingClientRect();
                        splatBurst(panel.left + panel.width / 2, panel.top + panel.height / 2, 26);
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
            gsap.fromTo(ul, { x: -10, rotate: -1 }, { x: 0, rotate: 0, duration: 0.4, ease: 'elastic.out(1,0.35)' });
        }
    }

    function toggle(sel, show) {
        const el = document.querySelector(sel);
        if (el) el.classList.toggle('hidden', !show);
    }
})();
