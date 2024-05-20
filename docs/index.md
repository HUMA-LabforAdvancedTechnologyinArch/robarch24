---
layout: default
title: About
---
# About

Human-in-the-loop design and fabrication chains have the potential to leverage machine intelligence while incorporating a holistic engagement of designers and fabricators. Harnessing the capabilities of humans within robotic fabrication processes could enhance versatility, robustness, and productivity. 

In this workshop, participants will learn about and engage in a cooperative and augmented human-robot design-to-assembly workflow, where humans and robots work in synergy; that is, by enabling the creation of a complex timber structure that would be unattainable through either's efforts alone. As such, participants will explore interactive human-in-the-loop computational design and fabrication workflows using phone-based augmented reality (AR). A holistic computational design approach will be demonstrated, where constraints such as structural stability, fabrication constraints, and human-robot task distribution are already considered in the design generation. This computational model is linked with an intuitive phone-based AR application enabling task distribution linking robotic and manual assembly. This AR interface informs and instructs humans on fabrication tasks and enables them to adjust and align robotic task distribution interactively. 

</br>

<figure>
  <img src="{{site.baseurl}}images/robarch24_01.jpg" alt="Human-Robot Cooperative Assembly of Spatial Structures." style="width:100%" class="center">
  <figcaption>Human-robot cooperative assembly of spatial structures using a custom-designed mobile Augmented Reality interface.</figcaption>
</figure>

Initially, a digital design tool guided by human input will be used to create the timber assembly's information model, considering fabrication constraints, structural stability, and the distribution of tasks between humans and robots, ensuring structural integrity and fabricability at each assembly step. During the augmented assembly, two mobile robots will precisely position timber members and occasionally provide temporary support at critical points in the timber structure. Human participants play an essential role in closing manually the reciprocal frames of the structure and adding mechanical connectors. Both humans and robots will share a digital-physical workspace, with humans receiving task instructions through a mobile AR interface, utilizing novel COMPAS XR features.

<figure>
  <img src="{{site.baseurl}}images/robarch24_02.jpg" alt="Human-Robot Cooperative Assembly of Spatial Structures." style="width:100%" class="center">
  <figcaption>Humans are instructed via AR for dedicated assembly tasks including adding mechanical connectors.</figcaption>
</figure>

As part of this workshop, we will show underlying data structures and software workflows connecting the geometric implementation of edge graphs with AR. It introduces Compas XR, a Compas extension package linking Compas and Compas Fab with XR capabilities for the first time. This custom-developed XR environment for mobile AR connects the CAD environment with a robotic fabrication control. It enables bi-directional interaction between the phone-based AR interface and the CAD environment. For this, the workshop combines multiple software environments such as CAD (Rhinoceros, Grasshopper, Compas and Python), robotic fabrication (Compas Fab and Python) with cloud-based servers (Firebase) and phone-based AR (Unity, ARCore and C#).

The demonstrated methods and approaches will be taught and applied by the workshop participants for the realization of a 1:1 scale timber structure designed for human-robot cooperation.
