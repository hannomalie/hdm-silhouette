﻿/*
* Farseer Physics Engine based on Box2D.XNA port:
* Copyright (c) 2010 Ian Qvist
* 
* Box2D.XNA port of Box2D:
* Copyright (c) 2009 Brandon Furtwangler, Nathan Furtwangler
*
* Original source Box2D:
* Copyright (c) 2006-2009 Erin Catto http://www.gphysics.com 
* 
* This software is provided 'as-is', without any express or implied 
* warranty.  In no event will the authors be held liable for any damages 
* arising from the use of this software. 
* Permission is granted to anyone to use this software for any purpose, 
* including commercial applications, and to alter it and redistribute it 
* freely, subject to the following restrictions: 
* 1. The origin of this software must not be misrepresented; you must not 
* claim that you wrote the original software. If you use this software 
* in a product, an acknowledgment in the product documentation would be 
* appreciated but is not required. 
* 2. Altered source versions must be plainly marked as such, and must not be 
* misrepresented as being the original software. 
* 3. This notice may not be removed or altered from any source distribution. 
*/

using FarseerPhysics.Collision;
using FarseerPhysics.Dynamics.Contacts;

namespace FarseerPhysics.Dynamics
{
    public delegate void BroadphaseDelegate(ref FixtureProxy proxyA, ref FixtureProxy proxyB);

    public class ContactManager
    {
        /// <summary>
        /// Fires when a contact is created
        /// </summary>
        public BeginContactDelegate BeginContact;

        public BroadPhase BroadPhase = new BroadPhase();

        public int ContactCount;

        public CollisionFilterDelegate ContactFilter;

        public Contact ContactList;

        /// <summary>
        /// Fires when a contact is deleted
        /// </summary>
        public EndContactDelegate EndContact;

        public BroadphaseDelegate OnBroadphaseCollision;

        /// <summary>
        /// Fires after the solver has run
        /// </summary>
        public PostSolveDelegate PostSolve;

        /// <summary>
        /// Fires before the solver runs
        /// </summary>
        public PreSolveDelegate PreSolve;

        internal ContactManager()
        {
            ContactList = null;
            ContactCount = 0;

            OnBroadphaseCollision = AddPair;
        }

        // Broad-phase callback.
        private void AddPair(ref FixtureProxy proxyA, ref FixtureProxy proxyB)
        {
            Fixture fixtureA = proxyA.Fixture;
            Fixture fixtureB = proxyB.Fixture;

            int indexA = proxyA.ChildIndex;
            int indexB = proxyB.ChildIndex;

            Body bodyA = fixtureA.Body;
            Body bodyB = fixtureB.Body;

            // Are the fixtures on the same body?
            if (bodyA == bodyB)
            {
                return;
            }

            // Does a contact already exist?
            ContactEdge edge = bodyB.ContactList;
            while (edge != null)
            {
                if (edge.Other == bodyA)
                {
                    Fixture fA = edge.Contact.FixtureA;
                    Fixture fB = edge.Contact.FixtureB;
                    int iA = edge.Contact.ChildIndexA;
                    int iB = edge.Contact.ChildIndexB;

                    if (fA == fixtureA && fB == fixtureB && iA == indexA && iB == indexB)
                    {
                        // A contact already exists.
                        return;
                    }

                    if (fA == fixtureB && fB == fixtureA && iA == indexB && iB == indexA)
                    {
                        // A contact already exists.
                        return;
                    }
                }

                edge = edge.Next;
            }

            // Does a joint override collision? Is at least one body dynamic?
            if (bodyB.ShouldCollide(bodyA) == false)
            {
                return;
            }

            // Check user filtering.
            if (ContactFilter != null && ContactFilter(fixtureA, fixtureB) == false)
            {
                return;
            }

            // Call the factory.
            Contact c = Contact.Create(fixtureA, indexA, fixtureB, indexB);

            // Contact creation may swap fixtures.
            fixtureA = c.FixtureA;
            fixtureB = c.FixtureB;
            indexA = c.ChildIndexA;
            indexB = c.ChildIndexB;
            bodyA = fixtureA.Body;
            bodyB = fixtureB.Body;

            // Insert into the world.
            c.Prev = null;
            c.Next = ContactList;
            if (ContactList != null)
            {
                ContactList.Prev = c;
            }
            ContactList = c;

            // Connect to island graph.

            // Connect to body A
            c.NodeA.Contact = c;
            c.NodeA.Other = bodyB;

            c.NodeA.Prev = null;
            c.NodeA.Next = bodyA.ContactList;
            if (bodyA.ContactList != null)
            {
                bodyA.ContactList.Prev = c.NodeA;
            }
            bodyA.ContactList = c.NodeA;

            // Connect to body B
            c.NodeB.Contact = c;
            c.NodeB.Other = bodyA;

            c.NodeB.Prev = null;
            c.NodeB.Next = bodyB.ContactList;
            if (bodyB.ContactList != null)
            {
                bodyB.ContactList.Prev = c.NodeB;
            }
            bodyB.ContactList = c.NodeB;

            ++ContactCount;
        }

        internal void FindNewContacts()
        {
            BroadPhase.UpdatePairs(OnBroadphaseCollision);
        }

        internal void Destroy(Contact contact)
        {
            Fixture fixtureA = contact.FixtureA;
            Fixture fixtureB = contact.FixtureB;
            Body bodyA = fixtureA.Body;
            Body bodyB = fixtureB.Body;

            if (EndContact != null && contact.IsTouching())
            {
                EndContact(contact);
            }

            // Remove from the world.
            if (contact.Prev != null)
            {
                contact.Prev.Next = contact.Next;
            }

            if (contact.Next != null)
            {
                contact.Next.Prev = contact.Prev;
            }

            if (contact == ContactList)
            {
                ContactList = contact.Next;
            }

            // Remove from body 1
            if (contact.NodeA.Prev != null)
            {
                contact.NodeA.Prev.Next = contact.NodeA.Next;
            }

            if (contact.NodeA.Next != null)
            {
                contact.NodeA.Next.Prev = contact.NodeA.Prev;
            }

            if (contact.NodeA == bodyA.ContactList)
            {
                bodyA.ContactList = contact.NodeA.Next;
            }

            // Remove from body 2
            if (contact.NodeB.Prev != null)
            {
                contact.NodeB.Prev.Next = contact.NodeB.Next;
            }

            if (contact.NodeB.Next != null)
            {
                contact.NodeB.Next.Prev = contact.NodeB.Prev;
            }

            if (contact.NodeB == bodyB.ContactList)
            {
                bodyB.ContactList = contact.NodeB.Next;
            }

            contact.Destroy();

            --ContactCount;
        }

        internal void Collide()
        {
            // Update awake contacts.
            Contact c = ContactList;
            while (c != null)
            {
                Fixture fixtureA = c.FixtureA;
                Fixture fixtureB = c.FixtureB;
                int indexA = c.ChildIndexA;
                int indexB = c.ChildIndexB;
                Body bodyA = fixtureA.Body;
                Body bodyB = fixtureB.Body;

                if (bodyA.Awake == false && bodyB.Awake == false)
                {
                    c = c.Next;
                    continue;
                }

                // Is this contact flagged for filtering?
                if ((c.Flags & ContactFlags.Filter) == ContactFlags.Filter)
                {
                    // Should these bodies collide?
                    if (bodyB.ShouldCollide(bodyA) == false)
                    {
                        Contact cNuke = c;
                        c = cNuke.Next;
                        Destroy(cNuke);
                        continue;
                    }

                    // Check user filtering.
                    if (ContactFilter != null && ContactFilter(fixtureA, fixtureB) == false)
                    {
                        Contact cNuke = c;
                        c = cNuke.Next;
                        Destroy(cNuke);
                        continue;
                    }

                    // Clear the filtering flag.
                    c.Flags &= ~ContactFlags.Filter;
                }

                int proxyIdA = fixtureA.Proxies[indexA].ProxyId;
                int proxyIdB = fixtureB.Proxies[indexB].ProxyId;

                bool overlap = BroadPhase.TestOverlap(proxyIdA, proxyIdB);

                // Here we destroy contacts that cease to overlap in the broad-phase.
                if (overlap == false)
                {
                    Contact cNuke = c;
                    c = cNuke.Next;
                    Destroy(cNuke);
                    continue;
                }

                // The contact persists.
                c.Update(this);
                c = c.Next;
            }
        }
    }
}